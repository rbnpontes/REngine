using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;

namespace REngine.RPI.Events
{
	public sealed class PipelineStateManagerEvents
	{
		public readonly AsyncEventEmitter OnReady = new();
		public readonly AsyncEventEmitter OnDispose = new();
		public readonly AsyncEventEmitter OnDisposed = new();

		public Task ExecuteReady(IPipelineStateManager manager) => OnReady.Invoke(manager);
		public Task ExecuteDispose(IPipelineStateManager manager) => OnDispose.Invoke(manager);
		public Task ExecuteDisposed(IPipelineStateManager manager) => OnDisposed.Invoke(manager);
	}
}
