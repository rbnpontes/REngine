using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.RHI;

namespace REngine.RPI.Events
{
	public sealed class RendererEvents
	{
		public event EventHandler? OnBeforeReady;
		public event EventHandler? OnReady;
		public event EventHandler? OnBeginCompile;
		public event EventHandler? OnEndCompile;
		public event EventHandler? OnBeginRender;
		public event EventHandler? OnEndRender;
		public event EventHandler? OnDispose;
		public event EventHandler? OnDisposed;
		public event EventHandler<ISwapChain>? OnChangeSwapChain;

		internal void ExecuteBeforeReady(IRenderer renderer)
		{
			OnBeforeReady?.Invoke(renderer, EventArgs.Empty);
		}
		internal void ExecuteReady(IRenderer renderer)
		{
			OnReady?.Invoke(renderer, EventArgs.Empty);
		}

		internal void ExecuteBeginCompile(IRenderer renderer)
		{
			OnBeginCompile?.Invoke(renderer, EventArgs.Empty);
		}

		internal void ExecuteEndCompile(IRenderer renderer)
		{
			OnEndCompile?.Invoke(renderer, EventArgs.Empty);
		}

		internal void ExecuteBeginRender(IRenderer renderer)
		{
			OnBeginRender?.Invoke(renderer, EventArgs.Empty);
		}

		internal void ExecuteEndRender(IRenderer renderer)
		{
			OnEndRender?.Invoke(renderer, EventArgs.Empty);
		}

		internal void ExecuteDispose(IRenderer renderer)
		{
			OnDispose?.Invoke(renderer, EventArgs.Empty);
		}

		internal void ExecuteDisposed(IRenderer renderer)
		{
			OnDisposed?.Invoke(renderer, EventArgs.Empty);
		}

		internal void ExecuteChangeSwapChain(IRenderer renderer, ISwapChain swapChain)
		{
			OnChangeSwapChain?.Invoke(renderer, swapChain);
		}
	}
}
