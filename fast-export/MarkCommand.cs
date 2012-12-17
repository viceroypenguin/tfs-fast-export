using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_export
{
	public abstract class MarkCommand : Command
	{
		public int? MarkId { get; protected set; }
		private bool _HasBeenRendered;

		public void RenderMarkReference(Stream stream)
		{
			if (!_HasBeenRendered)
				throw new InvalidOperationException("A MarkCommand cannot be referenced if it has not been rendered.");

			var reference = string.Format(":{0}", MarkId);
			stream.WriteString(reference);
		}

		protected void RenderMarkCommand(Stream stream)
		{
			if (MarkId != null)
			{
				var command = string.Format("mark :{0}", MarkId);
				stream.WriteLine(command);

				_HasBeenRendered = true;
			}
		}
	}
}
