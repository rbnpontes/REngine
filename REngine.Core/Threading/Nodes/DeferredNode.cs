using REngine.Core.DependencyInjection;
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
		private IEngine? pEngine;
		private double pLastTime = -1;

		public DeferredNode(ExecutionPipelineImpl execPipeline, IServiceProvider provider) : base(execPipeline, provider)
		{
		}

		public override void Execute()
		{
			object? varVal = pTargetValue.Value;

			if (varVal is null || varVal.GetType() != typeof(float))
				return;
			if(pEngine is null)
				pEngine = ServiceProvider.Get<IEngine>();

			if(pLastTime == -1)
			{
				ExecuteNode();
				pLastTime = pEngine.ElapsedTime;
				return;
			}

			float expectedTime = (float)varVal;
			float currTime = (float)(pEngine.ElapsedTime - pLastTime);
			if(currTime >= expectedTime)
			{
				ExecuteNode();
				pLastTime = pEngine.ElapsedTime;
			}
		}
		
		private void ExecuteNode()
		{
			base.Execute();
			ExecuteEvents();
			ExecuteChildrens();
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
