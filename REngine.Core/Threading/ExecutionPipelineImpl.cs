using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Threading.Nodes;
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
        private readonly ILogger<IExecutionPipeline> pLogger;

        private readonly Dictionary<ulong, ExecutionPipelineVarImpl> pVars = new Dictionary<ulong, ExecutionPipelineVarImpl>();
        private readonly ExecutionPipelineNodeRegistry pNodeRegistry;
        
        public readonly CancellationTokenSource StopTokenSource = new ();

        private IList<EPNode> pNodes = new List<EPNode>();
        private IDictionary<ulong, EPNode> pNodesTable = new Dictionary<ulong, EPNode>();
        private EPNode? pLastNode;
        private LinkedList<Action> pExecuteScheduledCalls = new();

        public ExecutionPipelineImpl(
            EngineEvents engineEvents,
            ILoggerFactory factory,
            ExecutionPipelineNodeRegistry nodeRegistry)
        {
            pNodeRegistry = nodeRegistry;
            pLogger = factory.Build<IExecutionPipeline>();

            engineEvents.OnBeforeStop += HandleStop;
        }

        private void HandleStop(object? sender, EventArgs e)
        {
            pLogger.Info("Stopping");
			StopTokenSource.Cancel();
            ClearAllEvents();
            pNodes.Clear();
            pNodesTable.Clear();
        }

        public IExecutionPipeline Load(Stream stream)
        {
            pLogger.Info("Loading Execution Pipeline Settings");
            EPResolver resolver = new(pNodeRegistry, this);
            resolver.Load(stream);

            pNodes = resolver.RootNodes;
            pNodesTable = resolver.NodesTable;

#if DEBUG
            // Clear registry to free memory
            pNodeRegistry.ClearRegistry();
#endif
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

            try
            {
                foreach (EPNode node in pNodes)
                    node.Execute();
            }
            catch (OperationCanceledException e)
            {
                pLogger.Info("Stop has been called. Stopping IExecutionPipeline", e.Message);
                return this;
            }
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
            return AddEvent(Hash.Digest(eventName), callback);
        }

        public IExecutionPipeline AddEvent(ulong eventHashCode, Action<IExecutionPipeline> callback)
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
            return RemoveEvent(Hash.Digest(eventName), callback);
        }

        public IExecutionPipeline RemoveEvent(ulong eventHashCode, Action<IExecutionPipeline> callback)
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
            return ClearEvents(Hash.Digest(eventName));
        }
        public IExecutionPipeline ClearEvents(ulong eventHashCode)
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
        
        public IExecutionPipelineVar GetOrCreateVar(string name)
        {
            return HandleCreateVar(Hash.Digest(name), name);
		}

        public IExecutionPipelineVar GetOrCreateVar(ulong varHashCode)
        {
            return HandleCreateVar(varHashCode);
        }

        private ExecutionPipelineVarImpl HandleCreateVar(ulong varHashCode, string dbgKey = "Unknow")
        {
            if (pVars.TryGetValue(varHashCode, out var varNode))
                return varNode;

			varNode = new ExecutionPipelineVarImpl(varHashCode);
#if DEBUG
			varNode.DebugKey = dbgKey;
#endif
            pVars[varHashCode] = varNode;
			return varNode;
		}
    }
}
