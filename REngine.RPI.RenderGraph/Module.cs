using REngine.Core.DependencyInjection;
using REngine.RPI.RenderGraph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public static class RenderGraphModule
	{
		public static RenderGraphRegistry GetBaseRegistry()
		{
			RenderGraphRegistry registry = new();
			registry
				.Register<IfNode>()
				.Register<InputNode>()
				.Register<VarNode>()
				.Register<SwitchNode>()
				.Register<DefaultNode>()
				.Register<CaseNode>();
			return registry;
		}
		public static void Setup(IServiceRegistry registry)
		{
			registry
				.Add(GetBaseRegistry)
				.Add<IVariableManager, VariableManager>()
				.Add<IResourceManager, ResourceManagerImpl>()
				.Add<IRenderGraph, RenderGraphImpl>();
		}
	}
}
