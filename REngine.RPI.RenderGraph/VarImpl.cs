using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public class VarChangeEventArgs : EventArgs
	{
		public object? OldValue { get; private set; }
		public object? NewValue { get; private set; }
		public VarChangeEventArgs(object? oldValue, object? newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	public interface IVar
	{
		public ulong Id { get; }
		public object? Value { get; set; }
		public event EventHandler<VarChangeEventArgs>? Change;
	}

	internal class VarImpl : IVar
	{
		private object? pValue;

#if DEBUG
		public string DebugName { get; internal set; }
#endif
		public ulong Id { get; internal set; }
		
		public object? Value
		{
			get => pValue;
			set
			{
				if(pValue != value)
				{
					var args = new VarChangeEventArgs(pValue, value);
					pValue = value;
					Change?.Invoke(this, args);
				}
			}
		}
		public event EventHandler<VarChangeEventArgs>? Change;

		public VarImpl(ulong varid)
		{
			Id = varid;
#if DEBUG
			DebugName = "Unknow";
#endif
		}
	}
}
