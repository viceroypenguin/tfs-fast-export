using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public class CommitterCommand : NameCommand
	{
		public override string CommandName
		{
			get { return "committer"; }
		}

		public CommitterCommand(string name, string email, DateTimeOffset date)
			: base(name, email, date) { }
	}
}
