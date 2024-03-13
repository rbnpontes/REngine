using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Events;

namespace REngine.Core
{
	public class UpdateEventArgs(IServiceProvider provider, IEngine engine, double deltaTime, double elapsed)
	{
		public double DeltaTime { get; set; } = deltaTime;
		public double Elapsed { get; set; } = elapsed;
		public IServiceProvider Provider { get; private set; } = provider;
		public IEngine Engine { get; private set; } = engine;
	}

	public sealed class EngineEvents
	{
		public readonly AsyncEventEmitter OnBeforeStart = new();
		public readonly AsyncEventEmitter OnStart = new();
		public readonly EventEmitter<UpdateEventArgs> OnUpdate = new();
		public readonly AsyncEventEmitter OnBeforeStop = new();
		public readonly AsyncEventEmitter OnStop = new();

		public EngineEvents()
		{
			ApplicationLifecyle.OnStart.Once(HandleAppStart);
			ApplicationLifecyle.OnExit.Once(HandleAppExit);
		}

		private async Task HandleAppExit(object sender)
		{
			await OnBeforeStop.Invoke(sender);
			await OnStop.Invoke(sender);
		}

		private async Task HandleAppStart(object sender)
		{
			await OnBeforeStart.Invoke(sender);
			await OnStart.Invoke(sender);
		}

		public EngineEvents ExecuteUpdate(UpdateEventArgs args)
		{
			OnUpdate.Invoke(this, args);
			return this;
		}
	}
}
