using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public class FileDeleteCommand : FileCommand
	{
		public FileDeleteCommand(string path)
		{
			base.Path = path;
		}

		public override void RenderCommand(Stream stream)
		{
			stream.WriteLine(string.Format("D {0}", Path));
		}
	}
}
