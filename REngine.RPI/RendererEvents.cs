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
	public class RendererUpdateSettingsEventArgs : RendererEventArgs
	{
		public RenderSettings Settings { get; private set; }
		public RendererUpdateSettingsEventArgs(IRenderer renderer, RenderSettings settings) : base(renderer)
		{
			Settings = settings;
		}
	}

	public class RendererEvents
	{
		public event EventHandler<RendererEventArgs>? OnStart;
		public event EventHandler<RendererEventArgs>? OnBeginRender;
		public event EventHandler<RendererEventArgs>? OnEndRender;
		public event EventHandler<RendererEventArgs>? OnChangeSwapChain;
		public event EventHandler<RendererEventArgs>? OnChangeBuffers;
		public event EventHandler<RendererUpdateSettingsEventArgs>? OnUpdateSettings;

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
			OnEndRender?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
		public RendererEvents ExecuteChangeSwapChain(IRenderer renderer)
		{
			OnChangeSwapChain?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
		public RendererEvents ExecuteChangeBuffers(IRenderer renderer)
		{
			OnChangeBuffers?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
		public RendererEvents ExecuteUpdateSettings(IRenderer renderer, RenderSettings settings)
		{
			OnUpdateSettings?.Invoke(this, new RendererUpdateSettingsEventArgs(renderer, settings));
			return this;
		}
		public RendererEvents ExecuteBeginDispose(IRenderer renderer)
		{
			OnBeginDispose?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
		public RendererEvents ExecuteEndDispose(IRenderer renderer)
		{
			OnEndDispose?.Invoke(this, new RendererEventArgs(renderer));
			return this;
		}
	}
}
