using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public class FileModifyCommand : FileCommand
	{
		public BlobCommand Blob { get; private set; }
		public FileModifyCommand(string path, BlobCommand blob)
		{
			base.Path = path;
			this.Blob = blob;
		}

		public DataCommand Data { get; private set; }
		public FileModifyCommand(string path, byte[] data)
		{
			base.Path = path;
			this.Data = new DataCommand(data);
		}

		public override void RenderCommand(Stream stream)
		{
			if (Blob != null)
			{
				stream.WriteString("M 644 ");
				Blob.RenderMarkReference(stream);
				stream.WriteString(" " + Path);
				stream.WriteLineFeed();
			}
			else
			{
				stream.WriteLine(string.Format("M 644 inline {0}", Path));
				stream.RenderCommand(Data);
			}
		}
	}
}
