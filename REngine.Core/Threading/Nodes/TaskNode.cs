using REngine.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace REngine.Core.Threading.Nodes
{
	[Node("task")]
	internal class TaskNode : EPNode
	{
		private const int MaxWaitTime = 1000 / 60;
		private readonly Action pTaskAction;
		private readonly ManualResetEventSlim pManualResetEvent = new (false);

		private EPNode? pTarget;
		
		public bool IsRunning { get; private set; }

		public TaskNode(ExecutionPipelineImpl execPipeline, IServiceProvider provider) : base(execPipeline, provider)
		{
			pTaskAction = ExecTask;
		}

		public override void Execute()
		{
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
