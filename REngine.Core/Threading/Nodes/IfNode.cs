using REngine.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using REngine.Core.IO;

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

		private bool pSkipExecution;
		private object? pLastValue;

#if PROFILER
		private string? pProfilerName;
#endif

		public ulong VarKey { get; set; }
		public IfNodeCmp Cmp { get; set; } = IfNodeCmp.Equal;
		

		public IfNode(ExecutionPipelineImpl execPipeline, IServiceProvider provider) : base(execPipeline, provider)
		{
		}

		public override void Execute()
		{
			if (VarKey == 0)
				return;

			pVar ??= ExecutionPipeline.GetOrCreateVar(VarKey);
			if (pLastValue != pVar.Value)
			{
				pSkipExecution = !CanExecute();
				pLastValue = pVar.Value;
			}

			if (pSkipExecution)
				return;
			base.Execute();
#if PROFILER
			pProfilerName ??= $"{nameof(IfNode)}#{GetHashCode()}:{DebugName}";
			using (Profiler.Instance.Begin(pProfilerName))
			{
#endif
				ExecuteEvents();
				ExecuteChildrens();
#if PROFILER
			}
#endif
		}

		private bool CanExecute()
		{
			var value = pVar?.Value;
			return value switch
			{
				bool boolValue => Validate(boolValue),
				Ref<bool> refBool => Validate(refBool.Value),
				_ => false
			};

			bool Validate(bool x)
			{
				return Cmp switch
				{
					IfNodeCmp.Equal => x,
					IfNodeCmp.NotEqual => x,
					_ => false
				};
			}
		}

		public override void Define(XmlElement element, Dictionary<ulong, EPNode> nodesList)
		{
			VarKey = Hash.Digest(element.GetAttribute("test"));
			string cmpValue = element.GetAttribute("compare");
			if (Enum.TryParse(cmpValue, out IfNodeCmp cmp))
				Cmp = cmp;
		}
	}
}
