using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public abstract class ResetCommand : Command
	{
		public string Reference { get; private set; }
		public CommitCommand From { get; private set; }
		public ResetCommand(string reference, CommitCommand from)
		{
			this.Reference = reference;
			this.From = from;
		}

		public override void RenderCommand(Stream stream)
		{
			stream.WriteLine(string.Format("reset {0}", Reference));
			if (From != null)
			{
				stream.WriteString("from ");
				From.RenderMarkReference(stream);
				stream.WriteLineFeed();
			}
		}
	}
}
