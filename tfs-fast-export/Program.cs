using System;
using System.Collections.Generic;
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
	class Program
	{
		private static HashSet<int> _SkipCommits = new HashSet<int>()
		{
			// use for skipping checkins that are unnecessary/outside the scope of branching
			// one example is build templates for TFS
		};

		private static HashSet<int> _BreakCommits = new HashSet<int>()
		{
			// use this for debugging when you want to stop at a particular checkin for analysis
		};

		static void Main(string[] args)
		{
			var collection = new TfsTeamProjectCollection(new Uri("http://server-name:8080/tfs/CollectionName"));
			collection.EnsureAuthenticated();
			var versionControl = collection.GetService<VersionControlServer>();

			var allChanges = versionControl
				.QueryHistory(
					"$/TFS-Root",
					VersionSpec.Latest,
					0,
					RecursionType.Full,
					null,
					new ChangesetVersionSpec(1),
					VersionSpec.Latest,
					int.MaxValue,
					true,
					false)
				.OfType<Changeset>()
				.OrderBy(x => x.ChangesetId)
				.ToList();

			var outStream = Console.OpenStandardOutput();
			foreach (var changeSet in allChanges)
			{
				if (_SkipCommits.Contains(changeSet.ChangesetId))
					continue;
				if (_BreakCommits.Contains(changeSet.ChangesetId))
					System.Diagnostics.Debugger.Break();

				var commit = new TfsChangeSet(changeSet).ProcessChangeSet();
				if (commit == null)
					continue;

				outStream.RenderCommand(commit);
				outStream.WriteLine(string.Format("progress {0}/{1}", changeSet.ChangesetId, allChanges.Last().ChangesetId));
			}
			outStream.WriteLine("done");
			outStream.Close();
		}
	}
}
