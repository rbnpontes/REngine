using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public interface ITextureData
	{
		public IntPtr Data { get; set; }
		public IBuffer? SrcBuffer { get; set; }
		public ulong SrcOffset { get; set; }
		public ulong Stride { get; set; }
		public ulong DepthStride { get; set; }
	}

	public class TextureData : ITextureData
	{
		public IntPtr Data { get; set; } = IntPtr.Zero;

		public IBuffer? SrcBuffer { get; set; } = null;

		public ulong SrcOffset { get; set; }

		public ulong Stride { get; set; }

		public ulong DepthStride { get; set; }
	}

	public class ByteTextureData : ITextureData
	{
		private GCHandle? pHandle;

		public IntPtr Data {
			get => pHandle?.AddrOfPinnedObject() ?? IntPtr.Zero;
			set => throw new NotSupportedException(); 
		}
		public IBuffer? SrcBuffer { get => null; set => throw new NotSupportedException(); }
		public ulong SrcOffset { get; set ; }
		public ulong Stride { get; set; }
		public ulong DepthStride { get; set; }

		public ByteTextureData() { }
		public ByteTextureData(byte[] data)
		{
			SetData(data);
		}
		public ByteTextureData(byte[] data, ulong stride)
		{
			SetData(data);
			Stride = stride;
		}
		~ByteTextureData()
		{
			pHandle?.Free();
			pHandle = null;
		}

		public void SetData(byte[] data)
		{
			pHandle?.Free();
			pHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

		}
		public byte[] GetData()
		{
			byte[] data = (byte[])(pHandle?.Target ?? new byte[0]);
			return data;
		}
	}
}
