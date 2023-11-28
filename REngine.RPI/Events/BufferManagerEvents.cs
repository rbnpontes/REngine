using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Events
{
	public sealed class BufferManagerEvents
	{
		public event EventHandler? OnChange;
		public event EventHandler? OnReady;
		public event EventHandler? OnDispose;
		public event EventHandler? OnDisposed;

		public void ExecuteReady(IBufferManager manager)
		{
			OnReady?.Invoke(manager, EventArgs.Empty);
		}
		
		public void ExecuteChange(IBufferManager manager)
		{
			OnChange?.Invoke(manager, EventArgs.Empty);
		}

		public void ExecuteDispose(IBufferManager manager)
		{
			OnDispose?.Invoke(manager, EventArgs.Empty);
		}

		public void ExecuteDisposed(IBufferManager manager)
		{
			OnDisposed?.Invoke(manager, EventArgs.Empty);
		}
	}
}
