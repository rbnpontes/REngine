using REngine.Core.Threading;
using System.Diagnostics;
using REngine.Core.DependencyInjection;
using REngine.Core.Events;
using REngine.Core.IO;
using Timer = REngine.Core.Timing.Timer;
namespace REngine.Core
{
	internal class EngineImpl : IEngine
	{
		private readonly Stopwatch pStopwatch;
		private readonly EngineEvents pEvents;
		private readonly UpdateEventArgs pUpdateEvtArgs;
		private readonly IExecutionPipeline pExecPipeline;
		private readonly EngineSettings pEngineSettings;
		private readonly IServiceProvider pServiceProvider;

		private readonly Timer pTimer = new();
		private IWindow? pMainWindow;

		private bool pStopped;
		public double DeltaTime { get => pTimer.DeltaTime; }
		public double ElapsedTime { get => pTimer.Elapsed; }

		public bool IsStopped { get => pStopped; }

		public EngineImpl(
			IServiceProvider provider,
			EngineEvents events,
			IExecutionPipeline pipeline,
			EngineSettings settings,
			IServiceProvider serviceProvider) 
		{
			pEvents	= events;
			pUpdateEvtArgs = new UpdateEventArgs(provider, this, 0, 0);
			pStopwatch = Stopwatch.StartNew();
			pExecPipeline = pipeline;
			pEngineSettings = settings;
			pServiceProvider = serviceProvider;
		}

		public IEngine Start()
		{
			Thread.CurrentThread.Name = "REngine - Main Thread";
			// Try get main window
			pMainWindow = pServiceProvider.GetOrDefault<IWindow>();

			pStopwatch.Restart();
			return this;
		}

		public IEngine ExecuteFrame()
		{
			if (pStopped)
				return this;

#if PROFILER
			Profiler.Instance.BeginFrame("Engine Frame");
#endif
			pTimer.Measure();

			pUpdateEvtArgs.DeltaTime = pTimer.DeltaTime;
			pUpdateEvtArgs.Elapsed = pTimer.Elapsed;

			pEvents.ExecuteUpdate(pUpdateEvtArgs);
			pExecPipeline.Execute();

			if (pTimer.DeltaTime <= pEngineSettings.GcCollectThreshold)
				GC.Collect();

			// If window is minimized, we don't want burn unnecessary CPU
			if (pMainWindow is { IsMinimized: true })
			{
				pExecPipeline.SetThreadSleep(pEngineSettings.IdleWaitTimeMs);
				Thread.Sleep(pEngineSettings.IdleWaitTimeMs);
			}
			else
			{
				pExecPipeline.SetThreadSleep(0);
			}

#if PROFILER
			Profiler.Instance.EndFrame();
#endif
			return this;
		}

		public IEngine Stop()
		{
			if (pStopped)
				return this;

			pStopped = true;
			ApplicationLifecyle.ExecuteExit();
			return this;
		}
	}
}
