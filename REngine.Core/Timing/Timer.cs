using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Timing
{
	/// <summary>
	/// Utility class used to measure delta time
	/// </summary>
	public sealed class Timer
	{
		private readonly Stopwatch pStopwatch = Stopwatch.StartNew();
		private readonly object pSync = new();

		private double pDeltaTime;
		private double pLastElapsed;
		
		public double DeltaTime { 
			get
			{
				double deltaTime;
				lock (pSync)
					deltaTime = pDeltaTime;
				return deltaTime;
			}
		}
		public double Elapsed { get => pStopwatch.Elapsed.TotalMilliseconds; }

		public void Measure()
		{
			lock (pSync)
			{
				double curr = pStopwatch.ElapsedMilliseconds * 0.001;
				pDeltaTime = curr - pLastElapsed;
				pLastElapsed = curr;
			}
		}
	}

    public sealed class TimerInterval
    {
		private readonly Action pAction;

		private float pTimeout;
		private double pLastElapsedTime;
		
		public TimerInterval(Action action)
		{
			pAction = action;
		}

		public TimerInterval SetInterval(float timeout)
		{
			pTimeout = timeout;
			return this;
		}

		public TimerInterval Update(double elapsedTime)
		{
			double diff = (elapsedTime - pLastElapsedTime);
			if (diff >= pTimeout)
			{
				pAction();
				pLastElapsedTime = elapsedTime;
			}
			return this;
		}
    }
}
