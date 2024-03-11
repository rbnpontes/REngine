using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Threading.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Events;

namespace REngine.Core.Threading
{
    internal class ExecutionPipelineImpl : IExecutionPipeline, IDisposable
    {
        // TODO: get rid of sync obj and use ConcurrentQueue instead
        private readonly object pSyncObj = new();
        private readonly ILogger<IExecutionPipeline> pLogger;
        private readonly IThreadCoordinator pCoordinator;
        private readonly ExecutionPipelineEvents pEvents;

        private readonly Dictionary<ulong, ExecutionPipelineVarImpl> pVars = new();
        private readonly ExecutionPipelineNodeRegistry pNodeRegistry;
        private readonly Action<EpNode> pExecNodeAction;
        private readonly Queue<Action> pExecuteMainThreadCalls = new();
        private readonly EngineSettings pEngineSettings;
        
        public readonly CancellationTokenSource StopTokenSource = new ();

        private bool pDisposed;

        private List<EpNode> pNodes = new ();
        private IDictionary<ulong, EpNode> pNodesTable = new Dictionary<ulong, EpNode>();
        private EpNode? pLastNode;

        public byte JobsCount => (byte)pCoordinator.JobsCount;
        
        public ExecutionPipelineImpl(
            EngineEvents engineEvents,
            ILoggerFactory factory,
            ExecutionPipelineNodeRegistry nodeRegistry,
            EngineSettings engineSettings,
            IThreadCoordinator threadCoordinator,
            ExecutionPipelineEvents execEvents)
        {
            pNodeRegistry = nodeRegistry;
            pLogger = factory.Build<IExecutionPipeline>();
            pCoordinator = threadCoordinator;
            pEngineSettings = engineSettings;
            pEvents = execEvents;

            engineEvents.OnBeforeStop.Once(HandleStop);

            pExecNodeAction = ExecuteNode;
        }

        private async Task HandleStop(object sender)
        {
            await EngineGlobals.MainDispatcher.Yield();
            pLogger.Info("Stopping");
            Dispose();
        }

        public void Dispose()
        {
	        if (pDisposed)
		        return;

            StopTokenSource.Cancel();
            ClearAllEvents();
            
            pNodes.Clear();
            pNodesTable.Clear();

            pEvents.ExecuteDispose(this);
            pDisposed = true;
        }

        public IExecutionPipeline Load(Stream stream)
        {
            pLogger.Info("Loading Execution Pipeline Settings");
            pEvents.ExecuteLoad(this);
            EPResolver resolver = new(pNodeRegistry, this);
            resolver.Load(stream);

            pNodes = resolver.RootNodes;
            pNodesTable = resolver.NodesTable;

#if DEBUG
            // Clear registry to free memory
            pNodeRegistry.ClearRegistry();
#endif
            pEvents.ExecuteLoaded(this);
            return this;
        }
        
        public IExecutionPipeline Execute()
        {
	        if (pDisposed)
		        return this;

            if (StopTokenSource.IsCancellationRequested)
            {
                lock (pSyncObj)
                    pExecuteMainThreadCalls.Clear();
                return this;
            }

            pCoordinator.Start(pEngineSettings.JobsThreadCount);

            // Execute Scheduled Calls
            while (true)
            {
	            Action? action;
	            lock (pSyncObj)
	            {
		            if (!pExecuteMainThreadCalls.TryDequeue(out action))
			            break;
	            }

	            action();
            }

            try
            { 
                pNodes.ForEach(pExecNodeAction);
            }
            catch (OperationCanceledException e)
            {
                pLogger.Info("Stop has been called. Stopping IExecutionPipeline", e.Message);
                return this;
            }
            return this;
        }

        private void ExecuteNode(EpNode node)
        {
            node.Execute();
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

        // FIXME: AddEvent is only work after load layout first
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
                pExecuteMainThreadCalls.Enqueue(action);
            return this;
        }

        public IExecutionPipeline Schedule(Action action)
        {
            // If job count is 0, then we must run this action at main thread
            // If current thread is not Main Thread, then we schedule action
            // to execute on main thread.
            if (pCoordinator.JobsCount == 0)
            {
                if (Thread.CurrentThread.Name == "REngine - Main Thread")
                    action();
                else
                    Invoke(action);
            }
            else
                pCoordinator.EnqueueAction(action);
	        return this;
        }

        public IExecutionPipeline SetThreadSleep(int threadSleep)
        {
            pCoordinator.SetThreadSleep(threadSleep);
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
