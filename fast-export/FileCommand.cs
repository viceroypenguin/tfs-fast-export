using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public abstract class FileCommand : Command
	{
		public string Path { get; protected set; }
	}
}
