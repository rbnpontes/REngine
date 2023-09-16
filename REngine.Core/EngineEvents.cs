using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public class UpdateEventArgs
	{
		public double DeltaTime { get; private set; }
		public double Elapsed { get; private set; }
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

	public delegate void EngineUpdateEvent(object sender, UpdateEventArgs args);

	public sealed class EngineEvents
	{
		public event EventHandler? OnStart;
		
		public event EngineUpdateEvent? OnBeginUpdate;
		public event EngineUpdateEvent? OnUpdate;
		public event EngineUpdateEvent? OnBeginRender;
		public event EngineUpdateEvent? OnRender;
		public event EngineUpdateEvent? OnEndRender;
		public event EngineUpdateEvent? OnEndUpdate;

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
		public EngineEvents ExecuteStop()
		{
			OnStop?.Invoke(this, EventArgs.Empty);
			return this;
		}
	}
}
