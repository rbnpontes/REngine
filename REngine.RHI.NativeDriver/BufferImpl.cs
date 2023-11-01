using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class BufferImpl : NativeObject, IBuffer
	{
		public BufferDesc Desc
		{
			get
			{
				BufferDescDTO dto = new();
				rengine_buffer_getdesc(Handle, ref dto);

				BufferDescDTO.Fill(dto, out BufferDesc result);
				return result;
			}
		}

		public ulong Size => Desc.Size;

		public string Name => Desc.Name;

		public GPUObjectType ObjectType { get; private set; }

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
		}
	}
}
