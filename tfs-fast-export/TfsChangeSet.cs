using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework;
using Microsoft.TeamFoundation.VersionControl.Client;

using fast_export;

namespace tfs_fast_export
{
	public class TfsChangeSet
	{
		private static Dictionary<string, Tuple<string, CommitCommand>> _Branches = new Dictionary<string, Tuple<string, CommitCommand>>();
		private static Dictionary<int, CommitCommand> _Commits = new Dictionary<int, CommitCommand>();

		private Changeset _ChangeSet;
		public TfsChangeSet(Changeset changeSet)
		{
			_ChangeSet = changeSet;
			@this = this;
		}

		// FYI this whole thing is entirely not thread-safe.
		private static TfsChangeSet @this;
		private static Dictionary<int, Func<CommitCommand>> _SpecialCommands = new Dictionary<int, Func<CommitCommand>>()
		{
			// use this to do checkin specific actions;  one example is when a branch itself changes name
			{ 12345, () =>
				{
					_Branches["$/Branch-A/"] = _Branches["$/Branch-B/"];
					_Branches.Remove("$/Branch-A/");
					return null;
				} },
		};
		public CommitCommand ProcessChangeSet()
		{
			if (_SpecialCommands.ContainsKey(_ChangeSet.ChangesetId))
				return _SpecialCommands[_ChangeSet.ChangesetId]();
			return DoProcessChangeSet();
		}

		private List<FileCommand> fileCommands = new List<FileCommand>();
		private List<CommitCommand> merges = new List<CommitCommand>();
		private string branch = null;
		private Func<Change, bool> _WhereClause = (x) => true;
		private CommitCommand DoProcessChangeSet()
		{
			var committer = new CommitterCommand(_ChangeSet.Committer, GetEmailAddressForUser(_ChangeSet.Committer), _ChangeSet.CreationDate);
			var author = _ChangeSet.Committer != _ChangeSet.Owner ? new AuthorCommand(_ChangeSet.Owner, GetEmailAddressForUser(_ChangeSet.Owner), _ChangeSet.CreationDate) : null;

			var orderedChanges = _ChangeSet.Changes
				.Where(_WhereClause)
				.Select((x, i) => new { x, i })
				.OrderBy(z => z.x.ChangeType)
				.ThenBy(z => z.i)
				.Select(z => z.x)
				.ToList();
			var deleteBranch = false;
			foreach (var change in orderedChanges)
			{
				var path = GetPath(change.Item.ServerItem);
				if (path == null)
					continue;

				// we delete before we check folders in case we can delete
				// an entire subdir w/ one command instead of file by file
				if ((change.ChangeType & ChangeType.Delete) == ChangeType.Delete)
				{
					fileCommands.Add(new FileDeleteCommand(path));
					if (path == "")
					{
						deleteBranch = true;
						break;
					}
					continue;
				}

				if (change.Item.ItemType == ItemType.Folder)
					continue;

				if ((change.ChangeType & ChangeType.Rename) == ChangeType.Rename)
				{
					var vcs = change.Item.VersionControlServer;
					var history = vcs
						.QueryHistory(
							change.Item.ServerItem,
							new ChangesetVersionSpec(_ChangeSet.ChangesetId),
							change.Item.DeletionId,
							RecursionType.None,
							null,
							null,
							new ChangesetVersionSpec(_ChangeSet.ChangesetId),
							int.MaxValue,
							true,
							false)
						.OfType<Changeset>()
						.ToList();

					var previousChangeset = history[1];
					var previousFile = previousChangeset.Changes[0];
					var previousPath = GetPath(previousFile.Item.ServerItem);
					fileCommands.Add(new FileRenameCommand(previousPath, path));

					// remove delete commands, since rename will take care of biz
					fileCommands.RemoveAll(fc => fc is FileDeleteCommand && fc.Path == previousPath);
				}

				var blob = GetDataBlob(change.Item);
				fileCommands.Add(new FileModifyCommand(path, blob));

				if ((change.ChangeType & ChangeType.Branch) == ChangeType.Branch)
				{
					var vcs = change.Item.VersionControlServer;
					var history = vcs.GetBranchHistory(new[] { new ItemSpec(change.Item.ServerItem, RecursionType.None) }, new ChangesetVersionSpec(_ChangeSet.ChangesetId));

					var itemHistory = history[0][0];
					var mergedItem = FindMergedItem(itemHistory, _ChangeSet.ChangesetId);
					var branchInfo = GetBranch(mergedItem.Relative.BranchFromItem.ServerItem).Item2;
					var previousCommit = branchInfo.Item2;
					if (!merges.Contains(previousCommit))
						merges.Add(previousCommit);
				}

				if ((change.ChangeType & ChangeType.Merge) == ChangeType.Merge)
				{
					var vcs = change.Item.VersionControlServer;
					var mergeHistory = vcs.QueryMergesExtended(new ItemSpec(change.Item.ServerItem, RecursionType.None), new ChangesetVersionSpec(_ChangeSet.ChangesetId), null, new ChangesetVersionSpec(_ChangeSet.ChangesetId)).ToList();
					foreach (var mh in mergeHistory)
					{
						var branchInfo = GetBranch(mh.SourceItem.Item.ServerItem).Item2;
						var previousCommit = branchInfo.Item2;
						if (!merges.Contains(previousCommit))
							merges.Add(previousCommit);
					}
				}
			}

			var reference = _Branches[branch];
			var commit = new CommitCommand(
				markId: _ChangeSet.ChangesetId,
				reference: reference.Item1,
				committer: committer,
				author: author,
				commitInfo: new DataCommand(_ChangeSet.Comment),
				fromCommit: reference.Item2,
				mergeCommits: merges,
				fileCommands: fileCommands);
			_Commits[_ChangeSet.ChangesetId] = commit;

			if (deleteBranch)
				_Branches.Remove(branch);
			else
				_Branches[branch] = Tuple.Create(reference.Item1, commit);

			return commit;
		}

		// 10,000,000 to get it out of way of normal checkins
		private static int _MarkID = 10000001;
		private BlobCommand GetDataBlob(Item item)
		{
			var bytes = new byte[item.ContentLength];
			var str = item.DownloadFile();
			str.Read(bytes, 0, bytes.Length);
			str.Close();

			var id = _MarkID++;
			var blob = BlobCommand.BuildBlob(bytes, id);
			return blob;
		}

		private static BranchHistoryTreeItem FindMergedItem(BranchHistoryTreeItem parent, int changeSetId)
		{
			foreach (BranchHistoryTreeItem item in parent.Children)
			{
				if (item.Relative.IsRequestedItem)
					return item;

				var x = FindMergedItem(item, changeSetId);
				if (x != null)
					return x;
			}
			return null;
		}

		private Tuple<string, Tuple<string, CommitCommand>> GetBranch(string serverPath)
		{
			foreach (var x in _Branches)
				if (serverPath.StartsWith(x.Key))
					return Tuple.Create(x.Key, x.Value);
			return null;
		}

		private string GetPath(string serverPath)
		{
			if (branch == null)
			{
				var branchInfo = GetBranch(serverPath);
				if (branchInfo == null)
				{
					CreateNewBranch(serverPath);
					return "";
				}
				else
					branch = branchInfo.Item1;
			}

			if (!serverPath.StartsWith(branch))
				// for now ignore secondary branches and hope that other filemodify commands work this stuff out
				return null;

			return serverPath.Replace(branch, "");
		}

		private void CreateNewBranch(string serverPath)
		{
			// Assumes that main directory for branch is the first thing added in new branch
			branch = serverPath + "/";

			if (!_Branches.ContainsKey(branch))
			{
				_Branches[branch] = Tuple.Create(string.Format("refs/heads/{0}", Path.GetFileName(serverPath)), default(CommitCommand));
				fileCommands.Add(new FileDeleteAllCommand());
			}
		}

		#region Active Directory
		private static string ProcessADName(string adName)
		{
			if (string.IsNullOrEmpty(adName))
				return "";

			if (!adName.Contains('\\'))
				return adName;

			var split = adName.Split('\\');
			return split[1];
		}

		private static UserPrincipal GetUserPrincipal(string userName)
		{
			var domainContext = new PrincipalContext(ContextType.Domain);
			var user = UserPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, ProcessADName(userName));
			if (user != null)
				return user;
			throw new InvalidOperationException(string.Format("Cannot find current user ({0}) in any domains.", userName));
		}

		private static string GetEmailAddressForUser(string userName)
		{
			try
			{
				return GetUserPrincipal(userName).EmailAddress;
			}
			catch
			{
				return "no.user@example.com";
			}
		}
		#endregion
	}
}
