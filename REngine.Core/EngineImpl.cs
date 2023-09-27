using REngine.Core.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	internal class EngineImpl : IEngine
	{
		private readonly Stopwatch pStopwatch;
		private readonly EngineEvents pEvents;
		private readonly UpdateEventArgs pUpdateEvtArgs;
		private readonly IExecutionPipeline pExecPipeline;

		private double pLastElapsed;
		private double pDeltaTime;
		private bool pStopped = false;

		public double DeltaTime { get => pDeltaTime; }
		public double ElapsedTime { get => pStopwatch?.Elapsed.TotalMilliseconds ?? 0.0; }
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

			double curr = pStopwatch.Elapsed.TotalMilliseconds;
			pDeltaTime = curr - pLastElapsed;
			pLastElapsed = curr;

			pUpdateEvtArgs.DeltaTime = pDeltaTime;
			pUpdateEvtArgs.Elapsed = curr;

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
