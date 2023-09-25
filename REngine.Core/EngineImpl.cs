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
		private readonly object pStepSync = new();
		private readonly object pRenderSync = new();
		private readonly UpdateEventArgs pUpdateEvtArgs;

		private EngineExecutionStep pStep = EngineExecutionStep.BeginFrame;

		private double pLastElapsed;
		private double pDeltaTime;
		private bool pStopped = false;
		private bool pExecuteRenderTask = false;

		private TaskCompletionSource pNextFrameSrc = new();
		private TaskCompletionSource pRenderSrc = new();

		private readonly CancellationTokenSource pStopToken = new();

		private Task? pPostRenderTask;

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

		public IEngine Start()
		{
			pPostRenderTask = Task.Factory.StartNew(
				HandlePostRenderTask, 
				pStopToken.Token, 
				TaskCreationOptions.LongRunning, 
				TaskScheduler.Default);
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

			pNextFrameSrc.SetResult();
			pNextFrameSrc = new TaskCompletionSource();
			
			AdvanceStep();
			pEvents.ExecuteBeginUpdate(pUpdateEvtArgs);
			AdvanceStep();
			pEvents.ExecuteUpdate(pUpdateEvtArgs);
			// Execute Works Here

			AdvanceStep();

			lock (pRenderSync)
			{
				pExecuteRenderTask = true;
				pEvents
					.ExecuteBeginRender(pUpdateEvtArgs)
					.ExecuteRender(pUpdateEvtArgs)
					.ExecuteEndRender(pUpdateEvtArgs);
			}

			pRenderSrc.SetResult();
			pRenderSrc = new TaskCompletionSource();

			// Execute Post Works Here
			pEvents.ExecuteEndUpdate(pUpdateEvtArgs);
			return this;
		}

		public IEngine Stop()
		{
			if (pStopped)
				return this;

			pStopToken.Cancel();
			try
			{
				pPostRenderTask?.Wait();
			}
			catch { }
			pEvents.ExecuteBeforeStop();
			pEvents.ExecuteStop();
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

		protected void HandlePostRenderTask()
		{
			while (true)
			{
				pStopToken.Token.ThrowIfCancellationRequested();
				lock (pRenderSync)
				{
					if (!pExecuteRenderTask)
						continue;
					pEvents.ExecuteAsyncRender(this);
					pExecuteRenderTask = false;
				}
				pStopToken.Token.ThrowIfCancellationRequested();
			}
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
