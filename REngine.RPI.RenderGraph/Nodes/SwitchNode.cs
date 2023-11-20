using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RPI.RenderGraph.Annotations;

namespace REngine.RPI.RenderGraph.Nodes
{
	[NodeTag("switch")]
	public sealed class SwitchNode : RenderGraphNode
	{
		public const string ValuePropertyKey = "value";

		public static readonly ulong ValuePropHash = Hash.Digest(ValuePropertyKey);

		private string pVarName = string.Empty;
		private IVar? pVar;

		public SwitchNode() : base(nameof(SwitchNode))
		{
		}

		protected override void OnSetup(IDictionary<ulong, string> properties)
		{
			if (!properties.TryGetValue(ValuePropHash, out var value))
				throw new RequiredNodePropertyException(ValuePropertyKey, nameof(SwitchNode));
			pVarName = value;
		}

		protected override void OnRun(IServiceProvider provider)
		{
			pVar ??= provider.Get<IVariableManager>().GetVar(pVarName);
			EvaluateCases(pVar);
		}

		private void EvaluateCases(IVar @var)
		{
			var children = Children;
			DefaultNode? defaultNode = null;
			var hasMatch = false;

			foreach (var child in children)
			{
				switch (child)
				{
					case DefaultNode node:
						defaultNode = node;
						break;
					case CaseNode caseNode:
						hasMatch |= caseNode.Evaluate(var);
						break;
				}
			}

			if(!hasMatch && defaultNode != null)
				defaultNode.SetRun(!hasMatch);
		}

		protected override bool OnAddChild(RenderGraphNode node)
		{
			return node is DefaultNode or CaseNode;
		}
	}

	public abstract class BaseCaseNode<T> : RenderGraphNode
	{
		protected bool mCanRun;
		internal BaseCaseNode() : base(nameof(T)){}

		protected override void OnRun(IServiceProvider provider)
		{
#if DEBUG
			if (Parent is null)
				throw new NullReferenceException($"{nameof(T)} must have a parent");
			if (Parent is not SwitchNode)
				throw new RenderGraphException($"{nameof(T)} parent must be {nameof(SwitchNode)}");
#endif
		}
		protected override IEnumerable<RenderGraphNode> OnGetChildren()
		{
			return mCanRun ? base.OnGetChildren() : Array.Empty<RenderGraphNode>();
		}
	}

	[NodeTag("default")]
	public sealed class DefaultNode : BaseCaseNode<DefaultNode>
	{
		public void SetRun(bool state)
		{
			mCanRun = state;
		}
	}

	[NodeTag("case")]
	public sealed class CaseNode : BaseCaseNode<DefaultNode>
	{
		public const string CasePropertyKey = "value";
		public static readonly ulong CasePropHash = Hash.Digest(CasePropertyKey);

		private int pValue;

		protected override void OnSetup(IDictionary<ulong, string> properties)
		{
			if (!properties.TryGetValue(CasePropHash, out var value))
				throw new RequiredNodePropertyException(CasePropertyKey, nameof(CaseNode));

			pValue = int.Parse(value);
		}

		public bool Evaluate(IVar @var)
		{
			mCanRun = false;
			var value = @var.Value;

			if (value is null)
				return mCanRun;

			return mCanRun = Equals(value, pValue);
		}
	}
}
