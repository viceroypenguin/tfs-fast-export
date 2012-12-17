using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public class FileRenameCommand : FileCommand
	{
		public string Source { get; private set; }
		public FileRenameCommand(string src, string dest)
		{
			this.Source = src;
			base.Path = dest;
		}

		public override void RenderCommand(Stream stream)
		{
			stream.WriteLine(string.Format("R {0} {1}", this.Source, this.Path));
		}
	}
}
