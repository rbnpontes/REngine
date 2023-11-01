using REngine.Core.DependencyInjection;
using REngine.RPI.RenderGraph.Annotations;
using REngine.RPI.RenderGraph.VarResolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph.Nodes
{
	[NodeTag("var")]
	public sealed class VarNode : RenderGraphNode
	{
		const string VarIdPropertyKey = "id";
		const string VarTypePropertyKey = "type";
		const string VarValuePropertyKey = "value";

		private static Dictionary<string, IVarValueResolver> sResolvers = new()
		{
			// Default Resolvers
			{ "bool", new BoolVarResolver() },
			{ "boolean", new BoolVarResolver() },
			{ "int", new IntVarResolver() },
			{ "float", new FloatVarResolver() },
			{ "double", new DoubleVarResolver() },
			{ "number", new DoubleVarResolver() },
		};

		private string pVarName = string.Empty;
		private string pType = string.Empty;
		private string pValue = string.Empty;

		public VarNode() : base(nameof(VarNode))
		{
		}

		protected override void OnSetup(IDictionary<int, string> properties)
		{
			if (!properties.TryGetValue(VarIdPropertyKey.GetHashCode(), out var id))
				throw new RequiredNodePropertyException(VarIdPropertyKey, nameof(VarNode));
			if (!properties.TryGetValue(VarTypePropertyKey.GetHashCode(), out var type))
				throw new RequiredNodePropertyException(VarTypePropertyKey, nameof(VarNode));
			if(properties.TryGetValue(VarValuePropertyKey.GetHashCode(), out var value))
				pValue = value;

			pVarName = id;
			pType = type;
		}

		protected override IEnumerable<RenderGraphNode> OnGetChildren() => Array.Empty<RenderGraphNode>();

		protected override void OnRun(IServiceProvider provider)
		{
			var varMgr = provider.Get<IVariableManager>();
			IVar varItem = varMgr.GetVar(pVarName);
			IVarValueResolver resolver = GetResolver(pType);
			varItem.Value = resolver.Resolve(pValue);
		}

		public static void AddResolver(string resolverType, IVarValueResolver resolver)
		{
			sResolvers[resolverType] = resolver;
		}
		
		public static IVarValueResolver GetResolver(string resolverType)
		{
			if (!sResolvers.TryGetValue(resolverType, out var resolver))
				throw new NotFoundVarResolver(resolverType);
			return resolver;
		}
	}
}
