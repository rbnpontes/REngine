using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.IO
{
	public static class PerformanceMeasure
	{
		private static readonly Dictionary<int, Stopwatch> sStopwatches = new();

		public static void BeginMeasure(int key)
		{
			sStopwatches[key] = Stopwatch.StartNew();
		}

		public static bool EndMeasure(int key, out TimeSpan timespan)
		{
			if (!sStopwatches.TryGetValue(key, out var stopwatch))
			{
				timespan = TimeSpan.Zero;
				return false;
			}

			stopwatch.Stop();
			timespan = stopwatch.Elapsed;
			return true;
		}
	}
}
