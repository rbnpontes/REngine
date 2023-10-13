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
		public event EventHandler? OnBeforeStart;
		public event EventHandler? OnStart;
		public event EventHandler<UpdateEventArgs>? OnUpdate;
		public event EventHandler? OnBeforeStop;
		public event EventHandler? OnStop;

		public EngineEvents ExecuteBeforeStart()
		{
			OnBeforeStart?.Invoke(this, EventArgs.Empty);
			return this;
		}

		public EngineEvents ExecuteStart()
		{
			OnStart?.Invoke(this, EventArgs.Empty);
			return this;
		}

		public EngineEvents ExecuteUpdate(UpdateEventArgs args)
		{
			OnUpdate?.Invoke(this, args);
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
