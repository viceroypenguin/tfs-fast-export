using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public abstract class Command
	{
		public abstract void RenderCommand(Stream stream);

		public static Encoding StreamEncoding { get; set; }
		static Command()
		{
			StreamEncoding = Encoding.UTF8;
		}
	}

	public static class CommandExtension
	{
		public static void WriteLine(this Stream stream, string s)
		{
			stream.WriteString(s);
			stream.WriteLineFeed();
		}

		public static void WriteString(this Stream stream, string s)
		{
			var bytes = Command.StreamEncoding.GetBytes(s);
			stream.Write(bytes, 0, bytes.Length);
		}

		private static byte[] _LineFeed = new byte[] { 0x0A };
		public static void WriteLineFeed(this Stream stream)
		{
			stream.Write(_LineFeed, 0, 1);
		}

		public static void RenderCommand(this Stream stream, Command c)
		{
			c.RenderCommand(stream);
		}
	}
}
