using REngine.Core.DependencyInjection;
using REngine.RHI;
using REngine.RPI.RenderGraph.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;

namespace REngine.RPI.RenderGraph.Nodes
{
	public enum IfNodeCompare
	{
		Equal = 0,
		NotEqual
	}

	[NodeTag("if")]
	public sealed class IfNode : RenderGraphNode
	{
		const string TestPropertyKey = "test";
		const string CmpPropertyKey = "compare";

		private readonly ulong TestPropHash = Hash.Digest(TestPropertyKey);
		private readonly ulong CmpPropHash = Hash.Digest(CmpPropertyKey);

		private bool pCanExecute = false;
		private string pVarName = string.Empty;
		private IVar? pVar = null;
		private IfNodeCompare pCompare = IfNodeCompare.Equal;

		public IfNode() : base(nameof(IfNode))
		{
		}

		protected override void OnSetup(IDictionary<ulong, string> properties)
		{
			if (!properties.TryGetValue(TestPropHash, out string? testVar))
				throw new RequiredNodePropertyException(TestPropertyKey, nameof(IfNode));
			if (!properties.TryGetValue(CmpPropHash, out string? cmp))
				cmp = "Equal";

			pVarName = testVar;
			Enum.TryParse(cmp, out pCompare);
		}

		protected override void OnRun(IServiceProvider provider)
		{
			if (pVar is null)
				pVar = provider.Get<IVariableManager>().GetVar(pVarName);

			ValidateVar(pVar);
		}

		private void ValidateVar(IVar vary)
		{
			object? value = vary.Value;
			if (value is null)
			{
				pCanExecute = false;
				return;
			}

			switch (pCompare)
			{
				case IfNodeCompare.Equal:
					pCanExecute = Equals(value, true);
					break;
				case IfNodeCompare.NotEqual:
					pCanExecute = !Equals(value, true);
					break;
			}
		}

		protected override IEnumerable<RenderGraphNode> OnGetChildren()
		{
			return pCanExecute ? base.OnGetChildren() : Array.Empty<RenderGraphNode>();
		}
		protected override void OnDispose()
		{
			pCanExecute = true;
		}
	}
}
