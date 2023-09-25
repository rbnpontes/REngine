using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public class UpdateEventArgs : EventArgs
	{
		public double DeltaTime { get; set; }
		public double Elapsed { get; set; }
		public IServiceProvider Provider { get; private set; }
		public IEngine Engine { get; private set; }

		public UpdateEventArgs(IServiceProvider provider, IEngine engine, double deltaTime, double elapsed)
		{
			Provider = provider;
			Engine = engine;
			DeltaTime = deltaTime;
			Elapsed = elapsed;
		}
	}

	public sealed class EngineEvents
	{
		public event EventHandler? OnStart;
		
		public event EventHandler<UpdateEventArgs>? OnBeginUpdate;
		public event EventHandler<UpdateEventArgs>? OnUpdate;
		public event EventHandler<UpdateEventArgs>? OnBeginRender;
		public event EventHandler<UpdateEventArgs>? OnRender;
		public event EventHandler<UpdateEventArgs>? OnEndRender;
		public event EventHandler<UpdateEventArgs>? OnEndUpdate;
		/// <summary>
		/// This event is called inside Task
		/// And task is completed before render
		/// </summary>
		public event EventHandler<EventArgs>? OnAsyncRender;

		public event EventHandler? OnBeforeStop;
		public event EventHandler? OnStop;

		public EngineEvents ExecuteStart()
		{
			OnStart?.Invoke(this, EventArgs.Empty);
			return this;
		}

		public EngineEvents ExecuteBeginUpdate(UpdateEventArgs args)
		{
			OnBeginUpdate?.Invoke(this, args);
			return this;
		}
		public EngineEvents ExecuteUpdate(UpdateEventArgs args)
		{
			OnUpdate?.Invoke(this, args);
			return this;
		}
		public EngineEvents ExecuteBeginRender(UpdateEventArgs args)
		{
			OnBeginRender?.Invoke(this, args);
			return this;
		}
		public EngineEvents ExecuteRender(UpdateEventArgs args)
		{
			OnRender?.Invoke(this, args);
			return this;
		}
		public EngineEvents ExecuteEndRender(UpdateEventArgs args)
		{
			OnEndRender?.Invoke(this, args);
			return this;
		}
		public EngineEvents ExecuteEndUpdate(UpdateEventArgs args)
		{
			OnEndUpdate?.Invoke(this, args);
			return this;
		}
		public EngineEvents ExecuteBeforeStop()
		{
			OnBeforeStop?.Invoke(this, EventArgs.Empty);
			return this;
		}
		public EngineEvents ExecuteAsyncRender(IEngine engine)
		{
			OnAsyncRender?.Invoke(engine, EventArgs.Empty);
			return this;
		}
		public EngineEvents ExecuteStop()
		{
			OnStop?.Invoke(this, EventArgs.Empty);
			return this;
		}
	}
}
