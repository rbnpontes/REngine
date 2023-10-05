using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public interface INativeObject : IDisposable
	{
		public IntPtr Handle { get; }
		public bool IsDisposed { get; }
		public event EventHandler? OnDispose;
	}
}
