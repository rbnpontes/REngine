using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace REngine.Core.Threading.Nodes
{
	internal abstract class EpNode(ExecutionPipelineImpl execPipeline, IServiceProvider provider)
	{
		private readonly object pSync = new();
		private readonly ConcurrentDictionary<int, Action<IExecutionPipeline>> pEvents = new();
		protected readonly ExecutionPipelineImpl ExecutionPipeline = execPipeline;
		protected readonly IServiceProvider ServiceProvider = provider;

		private bool pDirtyEvents;
		private Action<IExecutionPipeline>[] pEventCalls = [];
		public ulong Id { get; set; } = 0;
#if DEBUG || PROFILER
		public string DebugName { get; set; } = string.Empty;
		public string Xml { get; set; } = string.Empty;
#endif
		public List<EpNode> Children { get; set; } = new();
		public EpNode? Parent { get; set; } = null;

		public List<EpNode> LinkedNodes { get; set; } = new();

		public virtual void AddEvent(Action<IExecutionPipeline> listener)
		{
			lock (pSync)
			{
				pEvents.TryAdd(listener.GetHashCode(), listener);
				pDirtyEvents = true;
			}
		}

		public virtual void RemoveEvent(Action<IExecutionPipeline> listener)
		{
			lock (pSync)
			{
				pEvents.Remove(listener.GetHashCode(), out var _);
				pDirtyEvents = true;
			}
		}

		public virtual void Execute()
		{
			foreach (var item in LinkedNodes)
			{
				ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
				item.ExecuteLinkedNode(this);
			}
		}

		public virtual void ExecuteLinkedNode(EpNode owner)
		{
		}

		public virtual void ClearEvents()
		{
			pEvents.Clear();
		}

		/// <summary>
		/// Defines Execution Pipeline Node From XmlElement
		/// </summary>
		/// <param name="element"></param>
		/// <param name="nodesList"></param>
		/// <returns></returns>
		public abstract void Define(XmlElement element, Dictionary<ulong, EpNode> nodesList);

		protected void ExecuteEvents()
		{
			lock (pSync)
				ExecuteEventCalls();

			ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
		}
		
		private void ExecuteEventCalls()
		{
			if (pDirtyEvents)
			{
				pEventCalls = pEvents.Values.ToArray();
				pDirtyEvents = false;
			}

			foreach (var evtCall in pEventCalls)
			{
				ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
				evtCall(execPipeline);
			}
		}

		protected void ExecuteChildren()
		{
			foreach (var child in Children)
			{
				ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
				child.Execute();
			}
		}
	}
}
