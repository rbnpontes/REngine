using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public class RenderEventArgs : EventArgs
	{
		public IRenderer Renderer { get; private set; }

		public RenderEventArgs(IRenderer renderer)
		{
			Renderer = renderer;
		}
	}
	public class RenderUpdateSettingsEventArgs : RenderEventArgs
	{
		public RenderSettings Settings { get; private set; }
		public RenderUpdateSettingsEventArgs(IRenderer renderer, RenderSettings settings) : base(renderer)
		{
			Settings = settings;
		}
	}

	public class RenderEvents
	{
		public event EventHandler<RenderEventArgs>? OnStart;
		public event EventHandler<RenderEventArgs>? OnBeginRender;
		public event EventHandler<RenderEventArgs>? OnEndRender;
		public event EventHandler<RenderEventArgs>? OnChangeSwapChain;
		public event EventHandler<RenderEventArgs>? OnChangeBuffers;
		public event EventHandler<RenderUpdateSettingsEventArgs>? OnUpdateSettings;

		public event EventHandler<RenderEventArgs>? OnBeginDispose;
		public event EventHandler<RenderEventArgs>? OnEndDispose;

		public RenderEvents ExecuteStart(IRenderer renderer)
		{
			OnStart?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
		public RenderEvents ExecuteBeginRender(IRenderer renderer)
		{
			OnBeginRender?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
		public RenderEvents ExecuteEndRender(IRenderer renderer)
		{
			OnEndRender?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
		public RenderEvents ExecuteChangeSwapChain(IRenderer renderer)
		{
			OnChangeSwapChain?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
		public RenderEvents ExecuteChangeBuffers(IRenderer renderer)
		{
			OnChangeBuffers?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
		public RenderEvents ExecuteUpdateSettings(IRenderer renderer, RenderSettings settings)
		{
			OnUpdateSettings?.Invoke(this, new RenderUpdateSettingsEventArgs(renderer, settings));
			return this;
		}
		public RenderEvents ExecuteBeginDispose(IRenderer renderer)
		{
			OnBeginDispose?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
		public RenderEvents ExecuteEndDispose(IRenderer renderer)
		{
			OnEndDispose?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
	}
}
