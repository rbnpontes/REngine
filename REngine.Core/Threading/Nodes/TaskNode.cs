using REngine.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using REngine.Core.IO;

namespace REngine.Core.Threading.Nodes
{
	[Node("task")]
	internal class TaskNode : EPNode
	{
		private const int MaxWaitTime = 1000 / 60;
		private readonly Action pTaskAction;
		private readonly ManualResetEventSlim pManualResetEvent = new (false);

		private EPNode? pTarget;
#if PROFILER
		private string? pProfilerName;
		private IDisposable? pProfilerScope;
#endif
		public bool IsRunning { get; private set; }

		public TaskNode(ExecutionPipelineImpl execPipeline, IServiceProvider provider) : base(execPipeline, provider)
		{
			pTaskAction = ExecTask;
		}

		public override void Execute()
		{
#if PROFILER
			pProfilerName ??= $"{nameof(TaskNode)}#{GetHashCode()}:{DebugName}";
			pProfilerScope = Profiler.Instance.Begin(pProfilerName, ProfilerColor.Purple);
#endif
			ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
			pManualResetEvent.Reset();
			Task.Run(pTaskAction);
		}

		private void ExecTask()
		{
			IsRunning = true;
			ExecuteEvents();
			ExecuteChildrens();

			pManualResetEvent.Set();
		}

		public override void ExecuteLinkedNode(EPNode owner)
		{
			pManualResetEvent.Wait(MaxWaitTime);
#if PROFILER
			pProfilerScope?.Dispose();
			pProfilerScope = null;
#endif
			IsRunning = false;
		}

		public override void Define(XmlElement element, Dictionary<ulong, EPNode> nodesList)
		{
			var targetNodeId = Hash.Digest(element.GetAttribute("end"));
			if (targetNodeId == 0)
				return;

			if (nodesList.TryGetValue(targetNodeId, out pTarget))
				pTarget.LinkedNodes.Add(this);
		}
	}
}
