using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public class DataCommand : Command
	{
		public ReadOnlyCollection<byte> Bytes { get { return new ReadOnlyCollection<byte>(this._Bytes); } }
		internal byte[] _Bytes;
		public DataCommand(byte[] bytes)
		{
			this._Bytes = (byte[])bytes.Clone();
		}

		public DataCommand(string str)
		{
			this._Bytes = Command.StreamEncoding.GetBytes(str);
		}

		public override void RenderCommand(Stream stream)
		{
			var header = string.Format("data {0}", _Bytes.Length);
			stream.WriteLine(header);
			stream.Write(_Bytes, 0, _Bytes.Length);
			stream.WriteLineFeed();
		}
	}
}
