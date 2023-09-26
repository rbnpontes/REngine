using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Threading
{
    internal class ExecutionPipelineImpl : IExecutionPipeline
    {
        private IList<EPNode> pNodes = new List<EPNode>();
        private IDictionary<int, EPNode> pNodesTable = new Dictionary<int, EPNode>();
        private EPNode? pLastNode;

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
    }
}
