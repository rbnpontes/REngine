using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public class RendererEventArgs : EventArgs
	{
		public IRenderer Renderer { get; private set; }

		public RendererEventArgs(IRenderer renderer)
		{
			Renderer = renderer;
		}
	}

	public class RendererEvents
	{
		public event EventHandler<RendererEventArgs>? OnStart;
		public event EventHandler<RendererEventArgs>? OnBeginRender;
		public event EventHandler<RendererEventArgs>? OnEndRender;
		public event EventHandler<RendererEventArgs>? OnChangeSwapChain;
		public event EventHandler<RendererEventArgs>? OnChangeBuffers;
		public event EventHandler<RendererEventArgs>? OnUpdateSettings;

		public event EventHandler<RendererEventArgs>? OnBeginDispose;
		public event EventHandler<RendererEventArgs>? OnEndDispose;

		public RendererEvents ExecuteStart(IRenderer renderer)
		{
			OnStart?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
		public RendererEvents ExecuteBeginRender(IRenderer renderer)
		{
			OnBeginRender?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
		public RendererEvents ExecuteEndRender(IRenderer renderer)
		{
			OnBeginRender?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
		public RendererEvents ExecuteChangeSwapChain(IRenderer renderer)
		{
			OnBeginRender?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
		public RendererEvents ExecuteChangeBuffers(IRenderer renderer)
		{
			OnBeginRender?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
		public RendererEvents ExecuteUpdateSettings(IRenderer renderer)
		{
			OnBeginRender?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
		public RendererEvents ExecuteBeginDispose(IRenderer renderer)
		{
			OnBeginRender?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
		public RendererEvents ExecuteEndDispose(IRenderer renderer)
		{
			OnBeginRender?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
	}
}
