using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Events
{
	public sealed class PipelineStateManagerEvents
	{
		public event EventHandler? OnReady;
		public event EventHandler? OnDispose;
		public event EventHandler? OnDisposed;

		public void ExecuteReady(IPipelineStateManager manager)
		{
			OnReady?.Invoke(manager, EventArgs.Empty);
		}
		public void ExecuteDispose(IPipelineStateManager manager)
		{
			OnDispose?.Invoke(manager, EventArgs.Empty);
		}
		public void ExecuteDisposed(IPipelineStateManager manager)
		{
			OnDisposed?.Invoke(manager, EventArgs.Empty);
		}
	}
}
