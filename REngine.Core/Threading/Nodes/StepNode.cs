using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using REngine.Core.IO;

namespace REngine.Core.Threading.Nodes
{
	[Node("step")]
	internal class StepNode(ExecutionPipelineImpl execPipeline, IServiceProvider provider)
		: EpNode(execPipeline, provider)
	{

#if PROFILER
		private string? pProfilerName;
#endif
		public override void Define(XmlElement element, Dictionary<ulong, EpNode> nodesList)
		{
		}

		public override void Execute()
		{
			base.Execute();
#if PROFILER
			pProfilerName ??= $"{nameof(StepNode)}#{GetHashCode()}:{DebugName}";
			using (Profiler.Instance.Begin(pProfilerName, ProfilerColor.White))
			{
#endif
				ExecuteEvents();
				ExecuteChildren();
#if PROFILER
			}
#endif
		}
	}
}
