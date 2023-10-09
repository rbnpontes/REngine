using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Threading
{
	internal class ExecutionPipelineVarImpl : IExecutionPipelineVar
	{
		private object? pValue;
		private object pSync = new();

		public int Key { get; private set; }
		public object? Value
		{
			get
			{
				object? result;
				lock(pSync)
					result = pValue;
				return result;
			}
			set
			{
				lock(pSync)
					pValue = value;
			}
		}
		public ExecutionPipelineVarImpl(int varKey)
		{
			Key = varKey;
		}
	}
}
