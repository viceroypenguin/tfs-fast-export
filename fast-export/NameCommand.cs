using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public abstract class NameCommand : Command
	{
		public abstract string CommandName { get; }
		public string Name { get; private set; }
		public string Email { get; private set; }
		public DateTimeOffset Date { get; private set; }
		protected NameCommand(string name, string email, DateTimeOffset date)
		{
			this.Name = name;
			this.Email = email;
			this.Date = date;
		}

		private static long ToUnixTimestamp(DateTimeOffset dt)
		{
			DateTimeOffset unixRef = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0));
			return (dt.ToUniversalTime().Ticks - unixRef.Ticks) / 10000000;
		}

		private static string FormatDate(DateTimeOffset date)
		{
			var timestamp = ToUnixTimestamp(date);
			return string.Format("{0} +0000", timestamp);
		}

		public override void RenderCommand(Stream stream)
		{
			var command = CommandName;
			if (!string.IsNullOrEmpty(Name))
				command += " " + Name;
			command += string.Format(" <{0}> ", Email);
			command += FormatDate(Date);

			stream.WriteLine(command);
		}
	}
}
