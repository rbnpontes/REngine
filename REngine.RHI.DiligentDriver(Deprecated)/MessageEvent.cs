using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	public enum DbgMsgSeverity
	{
		Info,
		Warning,
		Error,
		FatalError
	}
	public class MessageEventArgs : EventArgs
	{
		public DbgMsgSeverity Severity { get; set; }
		public string Message { get; set; } = string.Empty;
		public string Function { get; set; } = string.Empty;
		public string File { get; set; } = string.Empty;
		public int Line { get; set; }
	}

	public delegate void MessageEvent(object sender, MessageEventArgs args);
}
