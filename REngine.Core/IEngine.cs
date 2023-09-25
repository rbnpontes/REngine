using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public enum EngineExecutionStep
	{
		BeginFrame = 0,
		Update,
		Render,
		Finish
	}
	public interface IEngine
	{
		public bool IsStopped { get; }
		public double DeltaTime { get; }
		public double ElapsedTime { get; }

		public EngineExecutionStep Step { get; }

		public IEngine Start();
		public IEngine ExecuteFrame();
		public IEngine Stop();
		public Task WaitRender();
		public Task WaitNextFrame();
	}
}
