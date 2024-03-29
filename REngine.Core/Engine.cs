﻿using REngine.Core.Threading;
using System.Diagnostics;
using REngine.Core.DependencyInjection;
using REngine.Core.Events;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using Timer = REngine.Core.Timing.Timer;
namespace REngine.Core
{
	public class Engine : IEngine
	{
		private readonly Stopwatch pStopwatch;
		private readonly EngineEvents pEvents;
		private readonly UpdateEventArgs pUpdateEvtArgs;
		private readonly IExecutionPipeline pExecPipeline;
		private readonly EngineSettings pEngineSettings;
		private readonly IServiceProvider pServiceProvider;
		private readonly IDispatcher pDispatcher;

		private readonly Timer pTimer = new();
		private IWindow? pMainWindow;

		private bool pStopped;

		protected ILogger<IEngine> Logger { get; private set; }
		
		public double DeltaTime => pTimer.DeltaTime;
		public double ElapsedTime => pTimer.Elapsed;

		public bool IsStopped => pStopped;
		public bool IsMainThread => !pDispatcher.IsThreadCaller;
		public virtual bool IsKeyboardVisible => false;

		public Engine(
			IServiceProvider provider,
			EngineEvents events,
			IExecutionPipeline pipeline,
			EngineSettings settings,
			ILoggerFactory loggerFactory,
			IDispatcher dispatcher) 
		{
			pEvents	= events;
			pUpdateEvtArgs = new UpdateEventArgs(provider, this, 0, 0);
			pStopwatch = Stopwatch.StartNew();
			pExecPipeline = pipeline;
			pEngineSettings = settings;
			pServiceProvider = provider;
			pDispatcher = dispatcher;
			Logger = loggerFactory.Build<IEngine>();
		}

		public async Task Start()
		{
			await pDispatcher.Yield();
			// Try get main window
			pMainWindow = pServiceProvider.GetOrDefault<IWindow>();
			pServiceProvider.Get<IExecutionPipeline>().AddEvent(DefaultEvents.GCCollect, CollectGC);

			pStopwatch.Restart();
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

			DisposableQueue.Dispose();
#if PROFILER
			Profiler.Instance.Plot("FPS", (1000.0f / pTimer.Milliseconds));
			Profiler.Instance.EndFrame();
#endif
			return this;
		}

		public async Task Stop()
		{
			await pDispatcher.Yield();
			if (pStopped)
				return;

			pStopped = true;
			await ApplicationLifecyle.ExecuteExit(this);
			await pDispatcher.Yield();
			
			while(DisposableQueue.HasPendingItems)
				DisposableQueue.Dispose();
		}

		public virtual IEngine ShowKeyboard()
		{
			PrintUnsupportedFeature(nameof(ShowKeyboard));
			return this;
		}

		public virtual IEngine HideKeyboard()
		{
			PrintUnsupportedFeature(nameof(HideKeyboard));
			return this;
		}

		protected void PrintUnsupportedFeature(string featureName)
		{
			Logger.Warning($"{featureName} is not supported on this platform");
		}

		private void CollectGC(IExecutionPipeline _)
		{
#if PROFILER
			using (Profiler.Instance.Begin(nameof(GC)))
#endif
			GC.Collect();
		}
	}
}
