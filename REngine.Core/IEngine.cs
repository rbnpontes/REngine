using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public interface IEngine
	{
		public bool IsStopped { get; }
		public double DeltaTime { get; }
		public double ElapsedTime { get; }

		public IEngine ExecuteFrame();
		public IEngine Stop();
	}
}
