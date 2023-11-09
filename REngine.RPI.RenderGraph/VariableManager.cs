using REngine.Core.Mathematics;
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
		public IVar GetVar(ulong varId);
		public IVariableManager SetVar(string name, object value);
		public IVariableManager SetVar(ulong varId, object value);
	}

	internal class VariableManager : IVariableManager
	{
		private Dictionary<ulong, VarImpl> pVars = new();

		private VarImpl GetOrCreateVar(ulong varId)
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
			VarImpl value = GetOrCreateVar(Hash.Digest(name));
#if DEBUG
			value.DebugName = name;
#endif
			return value;
		}

		public IVar GetVar(ulong varId)
		{
			return GetOrCreateVar(varId);
		}

		public IVariableManager SetVar(string name, object value)
		{
			return SetVar(Hash.Digest(name), value);
		}

		public IVariableManager SetVar(ulong varId, object value)
		{
			IVar varItem = GetVar(varId);
			varItem.Value = value;
			return this;
		}
	}
}
