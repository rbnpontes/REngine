using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	public interface INativeObject
	{
		public object? Handle { get; }
		public bool IsDisposed { get; }
	}
}
