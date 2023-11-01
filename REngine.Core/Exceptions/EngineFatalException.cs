using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Exceptions
{
	/// <summary>
	/// Engine Fatal Error. Generally this exception must not be throwed
	/// because fatal error means that some internal logic has been failed
	/// and program must exit, otherwise unexpected issues will occur.
	/// </summary>
	public class EngineFatalException : Exception
	{
		public EngineFatalException(string message) : base(message) { }
	}
}
