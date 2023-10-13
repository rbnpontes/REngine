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
		const int MaxWaitTime = 1000 / 60;
		private readonly object pSync = new();

		private Task? pIncomingTask;
		private EPNode? pTarget;

		public TaskNode(ExecutionPipelineImpl execPipeline, IServiceProvider provider) : base(execPipeline, provider)
		{
		}

		public override void Execute()
		{
			ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
			lock (pSync)
			{
				pIncomingTask = Task.Run(() =>
				{
					ExecuteEvents();
					ExecuteChildrens();
				});
				// Unset if there's not target
				if (pTarget is null)
					pIncomingTask = null;
			}
		}

		public override void AddEvent(Action<IExecutionPipeline> action)
		{
			int incomingTaskId;
			lock (pSync)
				incomingTaskId = pIncomingTask?.Id ?? -1;

			// Deadlock prevention
			if (Task.CurrentId != incomingTaskId)
				base.AddEvent(action);
			else
				pEvents.Clear();
		}

		public override void RemoveEvent(Action<IExecutionPipeline> action)
		{
			int incomingTaskId;
			lock (pSync)
			{
				incomingTaskId = pIncomingTask?.Id ?? -1;
			}
			// Deadlock prevention
			if (Task.CurrentId != incomingTaskId)
				base.RemoveEvent(action);
			else
				pEvents.Clear();
		}

		public override void ClearEvents()
		{
			int incomingTaskId;
			lock (pSync)
				incomingTaskId = pIncomingTask?.Id ?? -1;

			// If we call base, we have deadlock
			if (Task.CurrentId != incomingTaskId)
				base.ClearEvents();
			else
				pEvents.Clear();
		}

		public override void ExecuteLinkedNode(EPNode owner)
		{
			Task? task;

			lock (pSync)
			{
				task = pIncomingTask;
				pIncomingTask = null;
			}
			task?.Wait(MaxWaitTime);
		}

		public override void Define(XmlElement element, Dictionary<int, EPNode> nodesList)
		{
			int targetNodeId = element.GetAttribute("end").GetHashCode();
			if (targetNodeId == 0)
				return;

			nodesList.TryGetValue(targetNodeId, out pTarget);
		}
	}
}
