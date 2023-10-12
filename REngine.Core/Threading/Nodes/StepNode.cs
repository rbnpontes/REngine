using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace REngine.Core.Threading.Nodes
{
	[Node("step")]
	internal class StepNode : EPNode
	{
		public StepNode(ExecutionPipelineImpl execPipeline, IServiceProvider provider) : base(execPipeline, provider)
		{
		}

		public override void Define(XmlElement element, Dictionary<int, EPNode> nodesList)
		{
		}

		public override void Execute()
		{
			base.Execute();
			ExecuteEvents();
			ExecuteChildrens();
		}
	}
}
