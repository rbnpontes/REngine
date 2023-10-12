using REngine.Core.Threading;
using System.Diagnostics;
using Timer = REngine.Core.Timing.Timer;
namespace REngine.Core
{
	internal class EngineImpl : IEngine
	{
		private readonly Stopwatch pStopwatch;
		private readonly EngineEvents pEvents;
		private readonly UpdateEventArgs pUpdateEvtArgs;
		private readonly IExecutionPipeline pExecPipeline;
		private readonly object pSync = new();

		private readonly Timer pTimer = new Timer();

		private bool pStopped = false;


		public double DeltaTime { get => pTimer.DeltaTime; }
		public double ElapsedTime { get => pTimer.Elapsed; }

		public bool IsStopped { get => pStopped; }

		public EngineImpl(
			IServiceProvider provider,
			EngineEvents events,
			IExecutionPipeline pipeline) 
		{
			pEvents	= events;
			pUpdateEvtArgs = new UpdateEventArgs(provider, this, 0, 0);
			pStopwatch = Stopwatch.StartNew();
			pExecPipeline = pipeline;
		}

		public IEngine Start()
		{
			Thread.CurrentThread.Name = "REngine - Main Thread";
			pStopwatch.Restart();
			return this;
		}

		public IEngine ExecuteFrame()
		{
			if (pStopped)
				return this;

			pTimer.Measure();

			pUpdateEvtArgs.DeltaTime = pTimer.DeltaTime;
			pUpdateEvtArgs.Elapsed = pTimer.Elapsed;

			pEvents.ExecuteUpdate(pUpdateEvtArgs);
			pExecPipeline.Execute();

			return this;
		}

		public IEngine Stop()
		{
			if (pStopped)
				return this;

			pEvents.ExecuteBeforeStop();
			pEvents.ExecuteStop();
			pStopped = true;

			return this;
		}
	}
}
