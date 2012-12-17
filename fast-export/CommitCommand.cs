using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public class CommitCommand : MarkCommand
	{
		public string Reference { get; private set; }
		public AuthorCommand Author { get; private set; }
		public CommitterCommand Committer { get; private set; }
		public DataCommand CommitInfo { get; private set; }
		public CommitCommand FromCommit { get; private set; }
		public IList<CommitCommand> MergeCommits { get; private set; }
		public IList<FileCommand> FileCommands { get; private set; }

		public CommitCommand(
			int markId,
			string reference, 
			AuthorCommand author,
			CommitterCommand committer,
			DataCommand commitInfo,
			CommitCommand fromCommit,
			IList<CommitCommand> mergeCommits,
			IList<FileCommand> fileCommands)
		{
			if (string.IsNullOrEmpty(reference))
				throw new InvalidOperationException("The Reference for this commit must be valid.");
			if (committer == null)
				throw new InvalidOperationException("A committer must be specified for this commit.");
			if (commitInfo == null)
				throw new InvalidOperationException("Commit Information must be specified for this commit.");

			base.MarkId = markId;
			this.Reference = reference;
			this.Author = author;
			this.Committer = committer;
			this.CommitInfo = commitInfo;
			this.FromCommit = fromCommit;
			this.MergeCommits = (mergeCommits ?? new List<CommitCommand>()).ToList().AsReadOnly();
			this.FileCommands = (fileCommands ?? new List<FileCommand>()).ToList().AsReadOnly();
		}

		public override void RenderCommand(Stream stream)
		{
			foreach (var fc in FileCommands.OfType<FileModifyCommand>())
			{
				if (fc.Blob != null)
					stream.RenderCommand(fc.Blob);
			}

			stream.WriteLine(string.Format("commit {0}", Reference));
			base.RenderMarkCommand(stream);

			if (Author != null)
				stream.RenderCommand(Author);
			stream.RenderCommand(Committer);
			stream.RenderCommand(CommitInfo);

			if (FromCommit != null)
			{
				stream.WriteString("from ");
				FromCommit.RenderMarkReference(stream);
				stream.WriteLineFeed();
			}

			foreach (var mc in MergeCommits)
			{
				stream.WriteString("merge ");
				mc.RenderMarkReference(stream);
				stream.WriteLineFeed();
			}

			foreach (var fc in FileCommands)
				stream.RenderCommand(fc);
			stream.WriteLineFeed();
		} 
	}
}
