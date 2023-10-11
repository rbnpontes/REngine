using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace REngine.Core.Threading.Nodes
{
	[Node("deferred")]
	internal class DeferredNode : EPNode
	{
		private IExecutionPipelineVar pTargetValue = new ExecutionPipelineVarImpl(0);

		public DeferredNode(ExecutionPipelineImpl execPipeline) : base(execPipeline)
		{
		}

		public override void Execute()
		{
		}
		public override void Define(XmlElement element, Dictionary<int, EPNode> nodesList)
		{
			string targetValue = element.GetAttribute("target");
			if (string.IsNullOrEmpty(targetValue))
				throw new Exception($"'target' attribute is required. Xml: {element.OuterXml}");

			// if value is literal number, just fill var
			if(float.TryParse(targetValue, out float value))
			{
				pTargetValue.Value = value;
				return;
			}

			pTargetValue = ExecutionPipeline.GetOrCreateVar(targetValue);
		}
	}
}
