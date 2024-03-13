using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.RHI;

namespace REngine.RPI.Events
{
	public sealed class RendererEvents
	{
		public readonly AsyncEventEmitter OnBeforeReady = new();
		public readonly AsyncEventEmitter OnReady = new();
		public readonly AsyncEventEmitter OnBeginCompile = new();
		public readonly AsyncEventEmitter OnEndCompile = new();
		public readonly EventEmitter OnBeginRender = new();
		public readonly EventEmitter OnEndRender = new();
		public readonly AsyncEventEmitter OnDispose = new();
		public readonly AsyncEventEmitter OnDisposed = new();
		public readonly EventEmitter<ISwapChain> OnChangeSwapChain = new();

		internal Task ExecuteBeforeReady(IRenderer renderer) => OnBeforeReady.Invoke(renderer);
		internal Task ExecuteReady(IRenderer renderer) => OnReady.Invoke(renderer);

		internal Task ExecuteBeginCompile(IRenderer renderer) => OnBeginCompile.Invoke(renderer);

		internal Task ExecuteEndCompile(IRenderer renderer) => OnEndCompile.Invoke(renderer);

		internal void ExecuteBeginRender(IRenderer renderer) => OnBeginRender.Invoke(renderer);

		internal void ExecuteEndRender(IRenderer renderer) => OnEndRender.Invoke(renderer);

		internal Task ExecuteDispose(IRenderer renderer) => OnDispose.Invoke(renderer);

		internal Task ExecuteDisposed(IRenderer renderer) => OnDisposed.Invoke(renderer);

		internal void ExecuteChangeSwapChain(IRenderer renderer, ISwapChain swapChain) => OnChangeSwapChain.Invoke(renderer, swapChain);
	}
}
