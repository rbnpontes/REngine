﻿using REngine.Core.Mathematics;
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
	internal class TaskNode : EpNode
	{
		private const int MaxWaitTime = 1000 / 60;
		private readonly Action pTaskAction;
		private readonly ManualResetEventSlim pManualResetEvent = new (false);

		private EpNode? pTarget;
#if PROFILER
		private string? pProfilerName;
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
#endif
			ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
			pManualResetEvent.Reset();
			ExecutionPipeline.Schedule(pTaskAction);
		}

		private void ExecTask()
		{
#if PROFILER
			using (Profiler.Instance.Begin(pProfilerName))
			{
#endif
				IsRunning = true;
				ExecuteEvents();
				ExecuteChildren();
				pManualResetEvent.Set();
#if PROFILER
			}
#endif
		}

		public override void ExecuteLinkedNode(EpNode owner)
		{
			if (!IsRunning)
				return;
			pManualResetEvent.Wait(1000);
			IsRunning = false;
		}

		public override void Define(XmlElement element, Dictionary<ulong, EpNode> nodesList)
		{
			var targetNodeId = Hash.Digest(element.GetAttribute("end"));
			if (targetNodeId == 0)
				return;

			if (nodesList.TryGetValue(targetNodeId, out pTarget))
				pTarget.LinkedNodes.Add(this);
		}
	}
}
