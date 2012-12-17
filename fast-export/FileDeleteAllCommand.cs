using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public class FileDeleteAllCommand : FileCommand
	{
		public FileDeleteAllCommand()
		{
			base.Path = null;
		}

		public override void RenderCommand(Stream stream)
		{
			stream.WriteLine("deleteall");
		}
	}
}
