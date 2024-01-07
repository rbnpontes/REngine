using REngine.Core.DependencyInjection;
using REngine.RPI.RenderGraph.Annotations;
using REngine.RPI.RenderGraph.VarResolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;

namespace REngine.RPI.RenderGraph.Nodes
{
	[NodeTag("var")]
	public sealed class VarNode : RenderGraphNode
	{
		public const string IdPropertyKey = "id";
		public const string TypePropertyKey = "type";
		public const string VarValuePropertyKey = "value";

		public static readonly ulong IdPropHash = Hash.Digest(IdPropertyKey);
		public static readonly ulong TypePropHash = Hash.Digest(TypePropertyKey);
		public static readonly ulong ValuePropHash = Hash.Digest(VarValuePropertyKey);

		private static readonly Dictionary<string, IVarValueResolver> sResolvers = new()
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

		protected override void OnSetup(IDictionary<ulong, string> properties)
		{
			if (!properties.TryGetValue(IdPropHash, out var id))
				throw new RequiredNodePropertyException(IdPropertyKey, nameof(VarNode));
			if (!properties.TryGetValue(TypePropHash, out var type))
				throw new RequiredNodePropertyException(TypePropertyKey, nameof(VarNode));
			if(properties.TryGetValue(ValuePropHash, out var value))
				pValue = value;

			pVarName = id;
			pType = type;
		}

		protected override IReadOnlyList<RenderGraphNode> OnGetChildren() => [];

		protected override void OnRun(IServiceProvider provider)
		{
			var varMgr = provider.Get<IVariableManager>();
			IVar varItem = varMgr.GetVar(pVarName);
			IVarValueResolver resolver = GetResolver(pType);
			varItem.Value = resolver.Resolve(pValue);

			Dispose();
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
