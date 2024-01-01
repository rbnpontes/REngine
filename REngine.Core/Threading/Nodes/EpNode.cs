using System;
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
		private readonly HashSet<Action<IExecutionPipeline>> pEvents = new();
		protected readonly ExecutionPipelineImpl ExecutionPipeline = execPipeline;
		protected readonly IServiceProvider ServiceProvider = provider;

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
			lock(pSync)
				pEvents.Add(listener);
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

		public virtual void ExecuteLinkedNode(EpNode owner)
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
			var collection = pEvents.ToArray();
			foreach (var evtCall in collection)
			{
				ExecutionPipeline.StopTokenSource.Token.ThrowIfCancellationRequested();
				evtCall(ExecutionPipeline);
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
