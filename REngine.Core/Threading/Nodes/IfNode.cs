using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace REngine.Core.Threading.Nodes
{
	internal enum IfNodeCmp
	{
		Equal = 0,
		NotEqual,
	}

	[Node("if")]
	internal class IfNode : EPNode
	{
		private IExecutionPipelineVar? pVar;

		public int VarKey { get; set; } = 0;
		public IfNodeCmp Cmp { get; set; } = IfNodeCmp.Equal;
		
		public IfNode(ExecutionPipelineImpl execPipeline, IServiceProvider provider) : base(execPipeline, provider)
		{
		}

		public override void Execute()
		{
			if (VarKey == 0)
				return;

			if (pVar is null)
				pVar = ExecutionPipeline.GetOrCreateVar(VarKey);

			if (!CanExecute())
				return;
			base.Execute();
			ExecuteEvents();
			ExecuteChildrens();
		}

		private bool CanExecute()
		{
			object? value = pVar?.Value;
			if (value is null)
				return false;

			bool result = false;
			switch (Cmp)
			{
				case IfNodeCmp.Equal:
					result = Equals(value, true);
					break;
				case IfNodeCmp.NotEqual:
					result = !Equals(value, true);
					break;
			}

			return result;
		}

		public override void Define(XmlElement element, Dictionary<int, EPNode> nodesList)
		{
			VarKey = element.GetAttribute("test").GetHashCode();
			string cmpValue = element.GetAttribute("compare");
			if (Enum.TryParse(cmpValue, out IfNodeCmp cmp))
				Cmp = cmp;
		}
	}
}
