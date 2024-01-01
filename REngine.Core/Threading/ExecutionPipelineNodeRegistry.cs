using REngine.Core.Threading.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Threading
{
	internal class ExecutionPipelineNodeRegistry
	{
		private readonly Dictionary<string, Type> pTypes = new();
		private readonly IServiceProvider pServiceProvider;

		public ExecutionPipelineNodeRegistry(IServiceProvider provider)
		{
			pServiceProvider = provider;

			Add<StepNode>()
		   .Add<TaskNode>()
		   .Add<IfNode>()
		   .Add<DeferredNode>();
		}

		public ExecutionPipelineNodeRegistry Add<T>()
		{
			return Add(typeof(T));
		}

		public ExecutionPipelineNodeRegistry Add(Type type)
		{
			if (!type.IsSubclassOf(typeof(EpNode)))
				return this;
			NodeAttribute? attr = type.GetCustomAttribute<NodeAttribute>();
			if (attr is null)
				return this;
			pTypes[attr.Tag] = type;
			return this;
		}

		public EpNode Create(string tag, ExecutionPipelineImpl pipeline)
		{
			if (!pTypes.TryGetValue(tag, out var type))
				throw new Exception($"Invalid tag '{tag}'. There's no node registered with this tag");
			EpNode? node = Activator.CreateInstance(type, pipeline, pServiceProvider) as EpNode;
			if (node is null)
				throw new NullReferenceException($"Could not possible to create '{tag}' node.");
			return node;
		}
	
		public void ClearRegistry()
		{
			pTypes.Clear();
		}
	}
}
