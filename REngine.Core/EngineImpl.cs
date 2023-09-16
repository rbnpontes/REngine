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

		private double pLastElapsed;
		private double pDeltaTime;
		private bool pStopped = false;

		public double DeltaTime { get => pDeltaTime; }
		public bool IsStopped { get => pStopped; }

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

			pEvents
				.ExecuteBeginUpdate(args)
				.ExecuteUpdate(args);
			// Execute Works Here
			pEvents
				.ExecuteBeginRender(args)
				.ExecuteRender(args)
				.ExecuteEndRender(args);
			// Execute Post Works Here
			pEvents
				.ExecuteEndUpdate(args);
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
	}
}
