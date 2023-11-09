using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace REngine.Core.Threading.Nodes
{
	internal abstract class EPNode
	{
		private readonly object pSync = new();
		protected readonly LinkedList<Action<IExecutionPipeline>> pEvents = new();
		protected readonly ExecutionPipelineImpl ExecutionPipeline;
		protected readonly IServiceProvider ServiceProvider;

		public ulong Id { get; set; } = 0;
#if DEBUG
		public string DebugName { get; set; } = string.Empty;
		public string Xml { get; set; } = string.Empty;
#endif
		public List<EPNode> Children { get; set; } = new();
		public EPNode? Parent { get; set; } = null;

		public List<EPNode> LinkedNodes { get; set; } = new();

		public EPNode(ExecutionPipelineImpl execPipeline, IServiceProvider provider) 
		{
			ExecutionPipeline = execPipeline;
			ServiceProvider = provider;
		}

		public virtual void AddEvent(Action<IExecutionPipeline> listener)
		{
			lock(pSync)
				pEvents.AddLast(listener);
		}

		public virtual void RemoveEvent(Action<IExecutionPipeline> listener)
		{
			lock (pSync)
				pEvents.Remove(listener);
		}

		public virtual void Execute()
		{
			foreach (var item in LinkedNodes)
			{
				ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
				item.ExecuteLinkedNode(this);
			}
		}

		public virtual void ExecuteLinkedNode(EPNode owner)
		{
		}

		public virtual void ClearEvents()
		{
			lock (pSync)
				pEvents.Clear();
		}

		/// <summary>
		/// Defines Execution Pipeline Node From XmlElement
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public abstract void Define(XmlElement element, Dictionary<ulong, EPNode> nodesList);

		protected void ExecuteEvents()
		{
			ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();

			LinkedListNode<Action<IExecutionPipeline>>? nextEvent;
			lock (pSync)
				nextEvent = pEvents.First;

			while (nextEvent != null)
			{
				ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();

				Action<IExecutionPipeline> action = nextEvent.Value;
				action(ExecutionPipeline);
				nextEvent = nextEvent.Next;
			}

			ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
		}

		protected void ExecuteChildrens()
		{
			ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
			foreach (var child in Children)
				child.Execute();
		}
	}
}
