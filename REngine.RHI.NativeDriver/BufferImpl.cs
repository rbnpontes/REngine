using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class BufferImpl : NativeObject, IBuffer
	{
		private readonly BufferDesc pDesc;
		private readonly IBufferView?[] pViews = new IBufferView?[(int)BufferViewType.UnorderedAccess];
		
		public BufferDesc Desc => pDesc;

		public ulong Size => Desc.Size;

		public string Name => Desc.Name;

		public GPUObjectType ObjectType { get; }

		public ResourceState State
		{
			get => (ResourceState)rengine_buffer_get_state(Handle);
			set => rengine_buffer_set_state(Handle, (uint)value);
		}

		public ulong GPUHandle => rengine_buffer_get_gpuhandle(Handle);
		
		public BufferImpl(IntPtr handle) : base(handle)
		{
			var bindFlags = Desc.BindFlags;
			if ((bindFlags & BindFlags.VertexBuffer) != 0)
				ObjectType |= GPUObjectType.VertexBuffer;
			if ((bindFlags & BindFlags.IndexBuffer) != 0)
				ObjectType |= GPUObjectType.IndexBuffer;
			if ((bindFlags & BindFlags.UniformBuffer) != 0)
				ObjectType |= GPUObjectType.ConstantBuffer;
			if ((bindFlags & BindFlags.UnorderedAccess) != 0)
				ObjectType = GPUObjectType.UavBuffer;

			var dto = new BufferDescDTO();
			rengine_buffer_getdesc(handle, ref dto);

			BufferDescDTO.Fill(dto, ref pDesc);
		}
		
		public IBufferView GetDefaultView(BufferViewType viewType)
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
			var viewTypeIdx = (byte)(viewType - 1);
			var view = pViews[viewTypeIdx];
			if (view is not null)
				return view;

			var handle = rengine_buffer_get_default_view(Handle, viewTypeIdx);
			if (handle == IntPtr.Zero)
				throw new NullReferenceException($"Could not retrieve default view '{viewType}'.");
			view = new BufferViewImpl(handle, this);
			pViews[viewTypeIdx] = view;
			return view;
		}

		public IBufferView CreateView(BufferViewDesc desc)
		{
			ResultNative result = new();
			BufferViewCreateDescDTO.Fill(desc, out var ci);
			rengine_buffer_create_view(Handle, ref ci, ref result);

			if (result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ??
				                    "Error has occurred while is creating buffer view");
			if (result.value == IntPtr.Zero)
				throw new NullReferenceException("Error has occurred while is creating buffer view.");
			return new BufferViewImpl(result.value, this);
		}
	}
}
