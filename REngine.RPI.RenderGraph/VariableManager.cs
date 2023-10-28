using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public interface IVariableManager
	{
		public IVar GetVar(string name);
		public IVar GetVar(int varId);
		public IVariableManager SetVar(string name, object value);
		public IVariableManager SetVar(int varId, object value);
	}

	internal class VariableManager : IVariableManager
	{
		private Dictionary<int, VarImpl> pVars = new();

		private VarImpl GetOrCreateVar(int varId)
		{
			if(!pVars.TryGetValue(varId, out VarImpl? value))
			{
				value = new VarImpl(varId);
				pVars.Add(varId, value);
			}
			return value;
		}

		public IVar GetVar(string name)
		{
			VarImpl value = GetOrCreateVar(name.GetHashCode());
#if DEBUG
			value.DebugName = name;
#endif
			return value;
		}

		public IVar GetVar(int varId)
		{
			return GetOrCreateVar(varId);
		}

		public IVariableManager SetVar(string name, object value)
		{
			return SetVar(name.GetHashCode(), value);
		}

		public IVariableManager SetVar(int varId, object value)
		{
			IVar varItem = GetVar(varId);
			varItem.Value = value;
			return this;
		}
	}
}
