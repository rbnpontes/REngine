using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public class ResourceChangeEventArgs : EventArgs 
	{
		public IGPUObject? OldValue { get; private set; }
		public IGPUObject? NewValue { get; private set; }

		public ResourceChangeEventArgs(
			IGPUObject? oldValue,
			IGPUObject? newValue
		)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	public interface IResource
	{
		public int Id { get; }
		/// <summary>
		/// Return Resource Value
		/// If resource has not been set, this value will be null
		/// </summary>
		public IGPUObject? Value { get; }
		/// <summary>
		/// Emit if Value is changed
		/// </summary>
		public event EventHandler<ResourceChangeEventArgs>? ValueChanged;
	}

	internal class ResourceImpl : IResource
	{
		private IGPUObject? pObj;

		public IGPUObject? Value
		{
			get => pObj;
			set => pObj = value;
		}

		public int Id { get; internal set; }

#if DEBUG
		public string DebugName { get; internal set; } = string.Empty;
#endif

		public event EventHandler<ResourceChangeEventArgs>? ValueChanged;

		public ResourceImpl()
		{
		}
		public void Mutate(IGPUObject newValue)
		{
			if (newValue == pObj)
				return;
			var args = new ResourceChangeEventArgs(
				pObj,
				newValue
			);

			pObj = newValue;

			ValueChanged?.Invoke(this, args);
		}
	}
}
