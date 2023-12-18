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
		private IntPtr pData = IntPtr.Zero;

		public IntPtr Data {
			get => pData;
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
			if(pData != IntPtr.Zero)
				Marshal.FreeHGlobal(pData);
		}

		public unsafe void SetData(byte[] data)
		{
			if(pData != IntPtr.Zero)
				Marshal.FreeHGlobal(pData);

			var dataSize = data.Length * sizeof(byte);
			pData = Marshal.AllocHGlobal(dataSize);

			fixed (byte* ptr = data)
				Buffer.MemoryCopy(ptr, pData.ToPointer(), dataSize, dataSize);
		}
	}
}
