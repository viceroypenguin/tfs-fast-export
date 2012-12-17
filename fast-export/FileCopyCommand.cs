using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public class FileCopyCommand : FileCommand
	{
		public string Source { get; private set; }
		public FileCopyCommand(string src, string dest)
		{
			this.Source = src;
			base.Path = dest;
		}

		public override void RenderCommand(Stream stream)
		{
			stream.WriteLine(string.Format("C {0} {1}", this.Source, this.Path));
		}
	}
}
