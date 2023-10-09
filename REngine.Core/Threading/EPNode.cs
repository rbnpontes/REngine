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
		private readonly object pSyncObj = new();
		protected readonly LinkedList<Action<IExecutionPipeline>> pEvents = new();

		public int Id { get; set; } = 0;
#if DEBUG
		public string IdName { get; set; } = string.Empty;
#endif

		public List<EPNode> Children { get; set; } = new List<EPNode>();
		public EPNode? Parent { get; set; } = null;

		public List<EPNode> LinkedNodes { get; set; } = new List<EPNode>();

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
			foreach(var item in LinkedNodes)
			{
				pipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
				item.ExecuteLinkedNode(this);
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
		private EPNode? pTarget;

		public EPNode? Target
		{
			get
			{
				return pTarget;
			}
			set
			{
				if (pTarget == value || pTarget == this)
					return;

				pTarget?.LinkedNodes.Remove(this);
				value?.LinkedNodes.Add(this);
				pTarget = value;
			}
		}
		

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
				// Unset if there's not target
				if (pTarget is null)
					pIncomingTask = null;
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

	internal enum IfNodeCmp
	{
		Equal = 0,
		NotEqual,
	}

	internal class IfNode : EPNode
	{
		private IExecutionPipelineVar? pVar;

		public int VarKey { get; set; } = 0;
		public IfNodeCmp Cmp { get; set; } = IfNodeCmp.Equal;

		public IfNode(int id = 0) : base(id) { }
		public IfNode(EPNode parent) : base(parent) { }

		public override void Execute(ExecutionPipelineImpl pipeline)
		{
			if (VarKey == 0)
				return;

			if (pVar is null)
				pVar = pipeline.GetOrCreateVar(VarKey);

			if (!CanExecute())
				return;
			base.Execute(pipeline);
			ExecuteEvents(pipeline);
			ExecuteChildrens(pipeline);
		}

		private bool CanExecute()
		{
			object? value = pVar?.Value;
			if (value is null)
				return false;

			bool result = false;
			switch (Cmp)
			{
				case IfNodeCmp.Equal:
					result = Equals(value, true); 
					break;
				case IfNodeCmp.NotEqual:
					result = !Equals(value, true);
					break;
			}

			return result;
		}
	
		public static IfNode Resolve(XmlElement element)
		{
			string id = element.GetAttribute("id");
			int hashCode = id.GetHashCode();
			
			IfNode node = new(hashCode);
#if DEBUG
			node.IdName = id;
#endif
			node.VarKey = element.GetAttribute("test").GetHashCode();
			string cmpValue = element.GetAttribute("compare");
			if(Enum.TryParse(cmpValue, out IfNodeCmp cmp))
				node.Cmp = cmp;

			return node;
		}
	}
}
