using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Threading
{
    internal class ExecutionPipelineImpl : IExecutionPipeline
    {
        private static readonly LinkedList<Action> EmptyScheduledCalls = new();
        private readonly object pSyncObj = new();

        public readonly CancellationTokenSource StopTokenSource = new ();

        private IList<EPNode> pNodes = new List<EPNode>();
        private IDictionary<int, EPNode> pNodesTable = new Dictionary<int, EPNode>();
        private EPNode? pLastNode;
        private LinkedList<Action> pExecuteScheduledCalls = new();

        public ExecutionPipelineImpl(
            EngineEvents engineEvents)
        {
            engineEvents.OnBeforeStop += HandleStop;
        }

        private void HandleStop(object? sender, EventArgs e)
        {
			StopTokenSource.Cancel();
            ClearAllEvents();
            pNodes.Clear();
            pNodesTable.Clear();
        }

        public IExecutionPipeline Load(Stream stream)
        {
            EPResolver resolver = new();
            resolver.Load(stream);

            pNodes = resolver.Nodes;
            pNodesTable = resolver.NodesTable;

            return this;
        }
        
        public IExecutionPipeline Execute()
        {
            if (StopTokenSource.IsCancellationRequested)
            {
                lock (pSyncObj)
                    pExecuteScheduledCalls.Clear();
                return this;
            }

            LinkedList<Action> calls;
            lock (pSyncObj)
            {
                calls = pExecuteScheduledCalls;
                // Only recreate calls if count is greater than 0
                if (pExecuteScheduledCalls.Count == 0)
                    calls = EmptyScheduledCalls;
                else
                    pExecuteScheduledCalls = new LinkedList<Action>();
            }

            // Execute Calls
            LinkedListNode<Action>? nextCall = calls.First;
            while(nextCall != null)
            {
                nextCall.Value();
                nextCall = nextCall.Next;
            }

            foreach (EPNode node in pNodes)
                node.Execute(this);
            return this;
        }

        public IExecutionPipeline ClearAllEvents()
        {
            foreach(var node in pNodesTable)
                node.Value.ClearEvents();
            return this;
        }

        public IExecutionPipeline AddEvent(string eventName, Action<IExecutionPipeline> callback)
        {
            return AddEvent(eventName.GetHashCode(), callback);
        }

        public IExecutionPipeline AddEvent(int eventHashCode, Action<IExecutionPipeline> callback)
        {
            if(eventHashCode == pLastNode?.Id)
            {
                pLastNode.AddEvent(callback);
                return this;
            }

            pNodesTable.TryGetValue(eventHashCode, out pLastNode);
            pLastNode?.AddEvent(callback);
            return this;
        }

        public IExecutionPipeline RemoveEvent(string eventName, Action<IExecutionPipeline> callback)
        {
            return RemoveEvent(eventName.GetHashCode(), callback);
        }

        public IExecutionPipeline RemoveEvent(int eventHashCode, Action<IExecutionPipeline> callback)
        {
            if(eventHashCode == pLastNode?.Id)
            {
                pLastNode.RemoveEvent(callback);
                return this;
            }

            pNodesTable.TryGetValue(eventHashCode, out pLastNode);
            pLastNode?.RemoveEvent(callback);
            return this;
        }

        public IExecutionPipeline ClearEvents(string eventName)
        {
            return ClearEvents(eventName.GetHashCode());
        }
        public IExecutionPipeline ClearEvents(int eventHashCode)
        {
			if (eventHashCode == pLastNode?.Id)
			{
                pLastNode.ClearEvents();
				return this;
			}

			pNodesTable.TryGetValue(eventHashCode, out pLastNode);
			pLastNode?.ClearEvents();
			return this;
		}

        public IExecutionPipeline Invoke(Action action)
        {
            lock (pSyncObj)
                pExecuteScheduledCalls.AddLast(action);
            return this;
        }
    }
}
