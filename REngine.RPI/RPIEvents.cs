using REngine.RHI;
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
	public class RenderReadyEventArgs : RenderEventArgs 
	{
		public IGraphicsDriver Driver { get; private set; }
		public RenderReadyEventArgs(IRenderer renderer, IGraphicsDriver driver) : base(renderer)
		{
			Driver = driver;
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

	public class RPIEvents
	{
		public event EventHandler<RenderReadyEventArgs>? OnReady;
		public event EventHandler<RenderEventArgs>? OnBeginRender;
		public event EventHandler<RenderEventArgs>? OnEndRender;
		public event EventHandler<RenderEventArgs>? OnChangeSwapChain;
		public event EventHandler<EventArgs>? OnChangeBuffers;
		public event EventHandler<RenderUpdateSettingsEventArgs>? OnUpdateSettings;

		public event EventHandler<RenderEventArgs>? OnBeginDispose;
		public event EventHandler<RenderEventArgs>? OnEndDispose;

		public RPIEvents ExecuteReady(IRenderer renderer, IGraphicsDriver driver)
		{
			OnReady?.Invoke(this, new RenderReadyEventArgs(renderer, driver));
			return this;
		}
		public RPIEvents ExecuteBeginRender(IRenderer renderer)
		{
			OnBeginRender?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
		public RPIEvents ExecuteEndRender(IRenderer renderer)
		{
			OnEndRender?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
		public RPIEvents ExecuteChangeSwapChain(IRenderer renderer)
		{
			OnChangeSwapChain?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
		public RPIEvents ExecuteChangeBuffers(IBufferProvider provider)
		{
			OnChangeBuffers?.Invoke(provider, EventArgs.Empty);
			return this;
		}
		public RPIEvents ExecuteUpdateSettings(IRenderer renderer, RenderSettings settings)
		{
			OnUpdateSettings?.Invoke(this, new RenderUpdateSettingsEventArgs(renderer, settings));
			return this;
		}
		public RPIEvents ExecuteBeginDispose(IRenderer renderer)
		{
			OnBeginDispose?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
		public RPIEvents ExecuteEndDispose(IRenderer renderer)
		{
			OnEndDispose?.Invoke(this, new RenderEventArgs(renderer));
			return this;
		}
	}
}
