using REngine.Core.DependencyInjection;
using REngine.RHI;
using REngine.RPI.RenderGraph.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		const string IfNodePropertyKey = "test";
		const string IfNodeCmpPropertyKey = "compare";

		private bool pCanExecute = false;
		private string pVarName = string.Empty;
		private IVar? pVar = null;
		private IfNodeCompare pCompare = IfNodeCompare.Equal;

		public IfNode() : base(nameof(IfNode))
		{
		}

		protected override void OnSetup(IDictionary<int, string> properties)
		{
			if (!properties.TryGetValue(IfNodePropertyKey.GetHashCode(), out string? testVar))
				throw new RequiredNodePropertyException(IfNodePropertyKey, nameof(IfNode));
			if (!properties.TryGetValue(IfNodeCmpPropertyKey.GetHashCode(), out string? cmp))
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
