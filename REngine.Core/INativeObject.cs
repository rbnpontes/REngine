using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public interface INativeObject
	{
		public IntPtr Handle { get; }
	}
}
