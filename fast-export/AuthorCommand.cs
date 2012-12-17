using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public class AuthorCommand : NameCommand
	{
		public override string CommandName
		{
			get { return "author"; }
		}

		public AuthorCommand(string name, string email, DateTimeOffset date)
			: base(name, email, date) { }
	}
}
