using REngine.Core.DependencyInjection;
using REngine.Core.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using REngine.Core.IO;

namespace REngine.Core.Threading.Nodes
{
	[Node("deferred")]
	internal class DeferredNode : EPNode
	{
		private readonly TimerInterval pInterval;

		private IExecutionPipelineVar pTargetValue = new ExecutionPipelineVarImpl(0);
		private IEngine? pEngine;
#if PROFILER
		private string? pProfilerName;
#endif
		public DeferredNode(ExecutionPipelineImpl execPipeline, IServiceProvider provider) : base(execPipeline, provider)
		{
			pInterval = new TimerInterval(ExecuteNode);
		}

		public override void Execute()
		{
			object? varVal = pTargetValue.Value;

			if (varVal is null || varVal.GetType() != typeof(float))
				return;
			if(pEngine is null)
				pEngine = ServiceProvider.Get<IEngine>();

			pInterval
				.SetInterval((float)varVal)
				.Update(pEngine.ElapsedTime);
		}
		
		private void ExecuteNode()
		{
			base.Execute();
#if PROFILER
			pProfilerName ??= $"{nameof(DeferredNode)}#{GetHashCode()}:{DebugName}";
			using (Profiler.Instance.Begin(pProfilerName, ProfilerColor.Yellow))
			{
#endif
				ExecuteEvents();
				ExecuteChildrens();
#if PROFILER
			}
#endif
		}

		public override void Define(XmlElement element, Dictionary<ulong, EPNode> nodesList)
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
