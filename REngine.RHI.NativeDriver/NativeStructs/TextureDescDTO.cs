using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct TextureDescDTO
	{
		public IntPtr name;
		public byte dimension;
		public uint width;
		public uint height;
		public uint arraySizeOrDepth;
		public ushort format;
		public uint mipLevels;
		public uint sampleCount;
		public uint bindFlags;
		public byte usage;
		public byte accessFlags;
		public byte textureFlags;

		public ushort clear_format;
		public float clear_r;
		public float clear_g;
		public float clear_b;
		public float clear_a;

		public float clear_depth;
		public byte clear_stencil;

		public static void Fill(in TextureDesc desc, out TextureDescDTO output)
		{
			output = new TextureDescDTO
			{
				name = string.IsNullOrEmpty(desc.Name) ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(desc.Name),
				dimension = (byte)desc.Dimension,
				width = desc.Size.Width,
				height = desc.Size.Height,
				arraySizeOrDepth = desc.ArraySizeOrDepth,
				format = (ushort)desc.Format,
				mipLevels = desc.MipLevels,
				sampleCount = desc.SampleCount,
				bindFlags = (uint)desc.BindFlags,
				usage = (byte)desc.Usage,
				accessFlags = (byte)desc.AccessFlags,
				textureFlags = (byte)desc.Flags,

				clear_format = (ushort)desc.ClearValue.Format,
				clear_r = desc.ClearValue.R,
				clear_g = desc.ClearValue.G,
				clear_b = desc.ClearValue.B,
				clear_a = desc.ClearValue.A,
				clear_depth = desc.ClearValue.Depth,
				clear_stencil = desc.ClearValue.Stencil,
			};
		}

		public static void Fill(in TextureDescDTO desc, out TextureDesc output)
		{
			output = new TextureDesc
			{
				Name = Marshal.PtrToStringAnsi(desc.name) ?? string.Empty,
				Dimension = (TextureDimension)desc.dimension,
				Size = new TextureSize(desc.width, desc.height),
				ArraySizeOrDepth = desc.arraySizeOrDepth,
				Format = (TextureFormat)desc.format,
				MipLevels = desc.mipLevels,
				SampleCount = desc.sampleCount,
				BindFlags = (BindFlags)desc.bindFlags,
				Usage = (Usage)desc.usage,
				AccessFlags = (CpuAccessFlags)desc.accessFlags,
				Flags = (TextureFlags)desc.accessFlags,

				ClearValue = new TextureClearValue
				{
					R = desc.clear_r,
					G = desc.clear_g,
					B = desc.clear_b,
					A = desc.clear_a,
					Depth = desc.clear_depth,
					Stencil = desc.clear_stencil,
					Format = (TextureFormat)desc.format,
				}
			};
		}
	}

	public struct TextureViewDescDTO
	{
		public byte viewType;
		public byte dimension;
		public ushort format;
		public uint mostDetailedMip;
		public uint mipLevels;
		public uint firstSlice;
		public uint slicesCount;
		public byte accessFlags;
		public byte allowMipMapGeneration;

		public static void Fill(in TextureViewDesc desc, out TextureViewDescDTO output)
		{
			output = new TextureViewDescDTO
			{
				viewType = (byte)desc.ViewType,
				dimension = (byte)desc.Dimension,
				format = (ushort)desc.Format,
				mostDetailedMip = desc.MostDetailedMip,
				mipLevels = desc.MipLevels,
				firstSlice = desc.FirstSlice,
				slicesCount = desc.SlicesCount,
				accessFlags = (byte)desc.AccessFlags,
				allowMipMapGeneration = (byte)(desc.AllowMipMapGeneration ? 1 : 0),
			};
		}

		public static void Fill(in TextureViewDescDTO desc, out TextureViewDesc output)
		{
			output = new TextureViewDesc
			{
				ViewType = (TextureViewType)desc.viewType,
				Dimension = (TextureDimension)desc.dimension,
				Format = (TextureFormat)desc.format,
				MostDetailedMip = desc.mostDetailedMip,
				MipLevels = desc.mipLevels,
				FirstSlice = desc.firstSlice,
				SlicesCount = desc.slicesCount,
				AccessFlags = (UavAccessFlags)desc.accessFlags,
				AllowMipMapGeneration = desc.allowMipMapGeneration == 1,
			};
		}
	}

	public struct TextureDataDTO 
	{
		public IntPtr data;
		public IntPtr srcBuffer;
		public ulong srcOffset;
		public ulong stride;
		public ulong depthStride;

		public static void Fill(in ITextureData data, out TextureDataDTO output)
		{
			output = new TextureDataDTO
			{
				data = data.Data,
				srcBuffer = data.SrcBuffer?.Handle ?? IntPtr.Zero,
				srcOffset = data.SrcOffset,
				stride = data.Stride,
				depthStride = data.DepthStride
			};
		}
	}
}
