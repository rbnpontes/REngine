using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Windows
{
	public sealed class WindowsBuilder
	{
		public IWindow Build(Control? control = null)
		{
			return new ControlWrapper(control != null ? control : new Form() { StartPosition = FormStartPosition.CenterScreen });
		}
	}
}
