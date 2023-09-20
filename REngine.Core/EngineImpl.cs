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
		private Stopwatch? pStopwatch;
		private IServiceProvider pProvider;
		private EngineEvents pEvents;
		private object pStepSync = new object();
		private EngineExecutionStep pStep = EngineExecutionStep.BeginFrame;

		private double pLastElapsed;
		private double pDeltaTime;
		private bool pStopped = false;

		private TaskCompletionSource pNextFrameSrc = new TaskCompletionSource();
		private TaskCompletionSource pRenderSrc = new TaskCompletionSource();

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
			pProvider = provider;
			pEvents	= events;
		}

		public IEngine ExecuteFrame()
		{
			if (pStopped)
				return this;
			if (pStopwatch is null)
				pStopwatch = Stopwatch.StartNew();
			double curr = pStopwatch.Elapsed.TotalMilliseconds;
			pDeltaTime = curr - pLastElapsed;
			pLastElapsed = curr;

			UpdateEventArgs args = new UpdateEventArgs(pProvider, this, pDeltaTime, curr);

			pNextFrameSrc.SetResult();
			pNextFrameSrc = new TaskCompletionSource();

			AdvanceStep();
			pEvents.ExecuteBeginUpdate(args);
			AdvanceStep();
			pEvents.ExecuteUpdate(args);
			// Execute Works Here

			AdvanceStep();
			pEvents
				.ExecuteBeginRender(args)
				.ExecuteRender(args)
				.ExecuteEndRender(args);

			pRenderSrc.SetResult();
			pRenderSrc = new TaskCompletionSource();
			// Execute Post Works Here
			pEvents.ExecuteEndUpdate(args);
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
