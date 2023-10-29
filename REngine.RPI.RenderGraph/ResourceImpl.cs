using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public enum ResourceType
	{
		Unknow = 0,
		VertexBuffer,
		IndexBuffer,
		ConstantBuffer,
		RenderTarget,
		UnorderedAccess,
		DepthWrite,
		DepthRead,
		ShaderResource
	}

	public class ResourceChangeEventArgs : EventArgs 
	{
		public IGPUObject? OldValue { get; private set; }
		public IGPUObject? NewValue { get; private set; }
		public ResourceType OldType { get; private set; }
		public ResourceType NewType { get; private set; }

		public ResourceChangeEventArgs(
			IGPUObject? oldValue,
			IGPUObject? newValue,
			ResourceType oldType,
			ResourceType newType
		)
		{
			OldValue = oldValue;
			NewValue = newValue;
			OldType = oldType;
			NewType = newType;
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
		/// Get Resource Type
		/// </summary>
		public ResourceType Type { get; }
		/// <summary>
		/// Emit if Value is changed
		/// </summary>
		public event EventHandler<ResourceChangeEventArgs>? ValueChanged;
	}

	internal class ResourceImpl : IResource
	{
		private IGPUObject? pObj;
		private ResourceType pType;

		public IGPUObject? Value
		{
			get => pObj;
			set => pObj = value;
		}
		public ResourceType Type
		{
			get => pType;
			set => pType = value;
		}

		public int Id { get; internal set; }

#if DEBUG
		public string DebugName { get; internal set; } = string.Empty;
#endif

		public event EventHandler<ResourceChangeEventArgs>? ValueChanged;

		public ResourceImpl()
		{
			pType = ResourceType.Unknow;
		}

		public void Mutate(ResourceType resourceType)
		{
			Mutate(pObj, resourceType);
		}
		public void Mutate(IGPUObject newValue)
		{
			Mutate(newValue, pType);
		}
		public void Mutate(IGPUObject newValue, ResourceType newType)
		{
			if (newValue == pObj && newType == pType)
				return;
			var args = new ResourceChangeEventArgs(
				pObj,
				newValue,
				pType,
				newType
			);

			pType = newType;
			pObj = newValue;

			ValueChanged?.Invoke(this, args);
		}
	}
}
