using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct SwapChainDescNative
	{
		public uint Width;
		public uint Height;

		public ushort ColorFormat;
		public ushort DepthFormat;

		public uint Usage;
		public uint Transform;

		public uint BufferCount;
		public float DefaultDepthValue;
		public byte DefaultStencilValue;
		public int IsPrimary;

		public static void From(in SwapChainDesc desc, ref SwapChainDescNative output)
		{
			output.Width = desc.Size.Width;
			output.Height = desc.Size.Height;

			output.ColorFormat = (ushort)desc.Formats.Color;
			output.DepthFormat = (ushort)desc.Formats.Depth;

			output.Usage = (uint)desc.Usage;
			output.Transform = (uint)desc.Transform;

			output.BufferCount = desc.BufferCount;
			output.DefaultDepthValue = desc.DefaultDepthValue;
			output.DefaultStencilValue = desc.DefaultStencilValue;
			output.IsPrimary = desc.IsPrimary ? 1 : 0;
		}

		public static void CopyTo(in SwapChainDescNative desc, ref SwapChainDesc output)
		{
			output.Size = new SwapChainSize(desc.Width, desc.Height);

			output.Formats = new SwapChainFormats(
				(TextureFormat)desc.ColorFormat,
				(TextureFormat)desc.DepthFormat
			);

			output.Usage = (SwapChainUsage)desc.Usage;
			output.Transform = (SwapChainTransform)desc.Transform;

			output.BufferCount = desc.BufferCount;
			output.DefaultDepthValue = desc.DefaultDepthValue;
			output.DefaultStencilValue = desc.DefaultStencilValue;
			output.IsPrimary = desc.IsPrimary == 1;
		}
	}
}
