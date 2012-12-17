using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public class BlobCommand : MarkCommand
	{
		private class ByteComparer : IEqualityComparer<byte[]>
		{
			public bool Equals(byte[] x, byte[] y)
			{
				if (x.Length != y.Length) return false;
				for (int ix = 0; ix < x.Length; ++ix)
					if (x[ix] != y[ix]) return false;
				return true;
			}
			public int GetHashCode(byte[] obj)
			{
				int retval = 0;
				foreach (byte value in obj) retval = (retval << 6) ^ value;
				return retval;
			}
		}
		private static Dictionary<byte[], BlobCommand> _DataBlobs = new Dictionary<byte[], BlobCommand>(new ByteComparer());
		public static BlobCommand BuildBlob(byte[] data, int? markId)
		{
			var hasher = SHA1.Create();
			var hash = hasher.ComputeHash(data);
			if (_DataBlobs.ContainsKey(hash))
			{
				var blob = _DataBlobs[hash];
				if (blob.DataCommand._Bytes.Length != data.Length)
					throw new InvalidOperationException("There are two matching hashes, but the data are of two different lengths.");
				return blob;
			}
			else
			{
				var blob = new BlobCommand(data, markId);
				_DataBlobs[hash] = blob;
				return blob;
			}
		}

		public bool IsRendered { get; set; }
		public DataCommand DataCommand { get; private set; }
		public string Filename { get; private set; }
		private BlobCommand(DataCommand data, int? markId)
		{
			this.DataCommand = data;
			this.MarkId = markId;
			this.IsRendered = false;
		}

		private BlobCommand(byte[] data, int? markId)
			: this(new DataCommand(data), markId) { }

		public override void RenderCommand(Stream stream)
		{
			if (!IsRendered)
			{
				IsRendered = true;

				stream.WriteLine("blob");
				base.RenderMarkCommand(stream);
				stream.RenderCommand(DataCommand);
			}
		}
	}
}
