using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace REngine.Core.Threading
{
	internal abstract class EPNode
	{
		private object pSyncObj = new();
		protected readonly LinkedList<Action<IExecutionPipeline>> pEvents = new();

		public int Id { get; set; } = 0;
#if DEBUG
		public string IdName { get; set; } = string.Empty;
#endif

		public List<EPNode> Children { get; set; } = new List<EPNode>();
		public EPNode? Parent { get; set; } = null;

		public LinkedList<EPNode> LinkedNodes { get; set; } = new LinkedList<EPNode>();

		public EPNode(int id = 0) 
		{
			Id = id;
		}
		public EPNode(EPNode parent)
		{
			Parent = parent;
		}

		public virtual void AddEvent(Action<IExecutionPipeline> action)
		{
			lock (pSyncObj)
			{
				pEvents.AddLast(action);
			}
		}
		public virtual void RemoveEvent(Action<IExecutionPipeline> action)
		{
			lock (pSyncObj)
			{
				pEvents.Remove(action);
			}
		}

		public virtual void Execute(ExecutionPipelineImpl pipeline)
		{
			var nextLinkedNode = LinkedNodes.First;
			while(nextLinkedNode != null)
			{
				pipeline.StopTokenSource.Token.ThrowIfCancellationRequested();

				nextLinkedNode.Value.ExecuteLinkedNode(this);
				nextLinkedNode = nextLinkedNode.Next;
			}
		}

		public virtual void ExecuteLinkedNode(EPNode owner)
		{
		}
	
		public virtual void ClearEvents()
		{
			lock (pSyncObj)
			{
				pEvents.Clear();
			}
		}


		protected void ExecuteEvents(ExecutionPipelineImpl pipeline)
		{
			pipeline.StopTokenSource.Token.ThrowIfCancellationRequested();

			LinkedListNode<Action<IExecutionPipeline>>? nextEvent;
			lock (pSyncObj)
			{
				nextEvent = pEvents.First;
			}

			while(nextEvent != null)
			{
				pipeline.StopTokenSource.Token.ThrowIfCancellationRequested();

				Action<IExecutionPipeline> action = nextEvent.Value;
				action(pipeline);
				nextEvent = nextEvent.Next;
			}

			pipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
		}
		protected void ExecuteChildrens(ExecutionPipelineImpl pipeline)
		{
			pipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
			foreach (var child in Children)
				child.Execute(pipeline);
		}
	}

	internal class StepNode : EPNode
	{
		public StepNode(int id = 0) : base(id) { }
		public StepNode(EPNode parent) : base(parent) { }

		public override void Execute(ExecutionPipelineImpl executionPipeline)
		{
			base.Execute(executionPipeline);
			ExecuteEvents(executionPipeline);
			ExecuteChildrens(executionPipeline);
		}

		public static StepNode Resolve(XmlElement element)
		{
			string id = element.GetAttribute("id");
			int hashCode = id.GetHashCode();

			if(string.IsNullOrEmpty(id))
				throw new Exception($"Id attribute is required on <step/> element.\nCode: {element.OuterXml}");

			StepNode node = new(hashCode);
#if DEBUG
			node.IdName = id;
#endif
			return node;
		}

	}

	internal class TaskNode : EPNode
	{
		private readonly object pSyncObject = new();

		private Task? pIncomingTask;

		public EPNode? Target { get; set; }
		

		public TaskNode(int id = 0) : base(id) { }
		public TaskNode(EPNode parent) : base(parent) { }

		public override void Execute(ExecutionPipelineImpl pipeline)
		{
			pipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
			lock (pSyncObject)
			{
				pIncomingTask = Task.Run(() =>
				{
					ExecuteEvents(pipeline);
					ExecuteChildrens(pipeline);
				});

				if (Target is null)
					pIncomingTask = null;
				else
				{
					// Add self task to target node
					// So when node goes to be executed
					// The task will be waited first
					Target.LinkedNodes.AddLast(this);
				}
			}
		}

		public override void AddEvent(Action<IExecutionPipeline> action)
		{
			int incomingTaskId;
			lock (pSyncObject)
			{
				incomingTaskId = pIncomingTask?.Id ?? -1;
			}

			// Deadlock prevention
			if (Task.CurrentId != incomingTaskId)
				base.AddEvent(action);
			else
				pEvents.Clear();
		}
		
		public override void RemoveEvent(Action<IExecutionPipeline> action)
		{
			int incomingTaskId;
			lock (pSyncObject)
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
			lock (pSyncObject)
			{
				incomingTaskId = pIncomingTask?.Id ?? -1;
			}

			// If we call base, we have deadlock
			if (Task.CurrentId != incomingTaskId)
				base.ClearEvents();
			else
				pEvents.Clear();
		}

		public override void ExecuteLinkedNode(EPNode owner)
		{
			Task? task;
			lock (pSyncObject)
			{
				task = pIncomingTask;
				pIncomingTask = null;
			}
			task?.Wait();
		}

		public static TaskNode Resolve(XmlElement element, out int targetNodeId)
		{
			string id = element.GetAttribute("id");
			int hashCode = id.GetHashCode();

			if (string.IsNullOrEmpty(id))
				throw new Exception($"Id attribute is required on <task/> element.\nCode: {element.OuterXml}");

			string targetNode = element.GetAttribute("end");
			targetNodeId = targetNode.GetHashCode();

			if (targetNodeId == hashCode)
				throw new Exception("End attribute must not be equal to element id.");
			TaskNode result = new(hashCode);
#if DEBUG
			result.IdName = id;
#endif
			return result;
		}
	}
}
