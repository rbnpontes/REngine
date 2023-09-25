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
		private readonly object pStepSync = new object();
		private EngineExecutionStep pStep = EngineExecutionStep.BeginFrame;

		private double pLastElapsed;
		private double pDeltaTime;
		private bool pStopped = false;

		private TaskCompletionSource pNextFrameSrc = new TaskCompletionSource();
		private TaskCompletionSource pRenderSrc = new TaskCompletionSource();

		private readonly UpdateEventArgs pUpdateEvtArgs;

		public double DeltaTime { get => pDeltaTime; }
		public double ElapsedTime { get => pStopwatch?.Elapsed.TotalMilliseconds ?? 0.0; }
		public bool IsStopped { get => pStopped; }

		public EngineExecutionStep Step
		{
			get
			{
				EngineExecutionStep step;
				lock (pStepSync)
				{
					step = pStep;
				}
				return step;
			}
		}

		public EngineImpl(IServiceProvider provider, EngineEvents events) 
		{
			pEvents	= events;
			pUpdateEvtArgs = new UpdateEventArgs(provider, this, 0, 0);
			pStopwatch = Stopwatch.StartNew();
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

			pNextFrameSrc.SetResult();
			pNextFrameSrc = new TaskCompletionSource();
			
			AdvanceStep();
			pEvents.ExecuteBeginUpdate(pUpdateEvtArgs);
			AdvanceStep();
			pEvents.ExecuteUpdate(pUpdateEvtArgs);
			// Execute Works Here

			AdvanceStep();
			pEvents
				.ExecuteBeginRender(pUpdateEvtArgs)
				.ExecuteRender(pUpdateEvtArgs)
				.ExecuteEndRender(pUpdateEvtArgs);

			pRenderSrc.SetResult();
			pRenderSrc = new TaskCompletionSource();
			// Execute Post Works Here
			pEvents.ExecuteEndUpdate(pUpdateEvtArgs);
			return this;
		}

		public IEngine Stop()
		{
			if (!pStopped)
			{
				pEvents.ExecuteBeforeStop();
				pEvents.ExecuteStop();
			}
			pStopped = true;

			return this;
		}

		public Task WaitRender()
		{
			return pRenderSrc.Task;
		}
		public Task WaitNextFrame()
		{
			return pNextFrameSrc.Task;
		}

		private void AdvanceStep()
		{
			lock (pStepSync)
			{
				var step = pStep;
				if(step == EngineExecutionStep.Finish)
				{
					step = EngineExecutionStep.BeginFrame;
				}
				else
				{
					step++;
				}
				pStep = step;
			} 
		}
	}
}
