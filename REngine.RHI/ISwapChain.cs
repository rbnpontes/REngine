using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	[Flags]
	public enum SwapChainUsage
	{
		None = 0x0,
		RenderTarget = 0x1,
		ShaderResource = 0x2,
		InputAttachment = 0x4,
		CopySource = 0x8,
	}

	public enum SwapChainTransform
	{
		Optimal,
		Identity,
		Rotate90,
		Rotate180,
		Rotate270,
		HorizontalMirror,
		HorizontalMirrorRotate90,
		HorizontalMirrorRotate180,
		HorizontalMirrorRotate270
	}
	
	public struct SwapChainSize
	{
		public uint Width;
		public uint Height;

		public SwapChainSize()
		{
			Width = Height = 0;
		}
		public SwapChainSize(uint width, uint height)
		{
			Width = width;
			Height = height;
		}
	}
	
	public struct SwapChainFormats
	{
		public TextureFormat Color;
		public TextureFormat Depth;

		public static readonly SwapChainFormats None = new SwapChainFormats();
		public static readonly SwapChainFormats RGBA = new SwapChainFormats
		{
			Color = TextureFormat.RGBA8UNorm,
			Depth = TextureFormat.D32Float,
		};
		public static readonly SwapChainFormats RGBASrgb = new SwapChainFormats
		{
			Color = TextureFormat.RGBA8UNormSRGB,
			Depth = TextureFormat.D32Float
		};
		public static readonly SwapChainFormats BGRA = new SwapChainFormats
		{
			Color = TextureFormat.BGRA8UNorm,
			Depth = TextureFormat.D32Float
		};
		public static readonly SwapChainFormats BGRASrgb = new SwapChainFormats
		{
			Color = TextureFormat.BGRA8UNormSRGB,
			Depth = TextureFormat.D32Float
		};
	}

	public struct SwapChainDesc
	{
		public SwapChainSize Size;
		public SwapChainFormats Formats;
		public SwapChainUsage Usage;
		public SwapChainTransform Transform;
		public uint BufferCount;
		public float DefaultDepthValue;
		public byte DefaultStencilValue;
		public bool IsPrimary;

		public SwapChainDesc()
		{
			Size = new SwapChainSize();
			Formats = SwapChainFormats.RGBA;
			Usage = SwapChainUsage.RenderTarget | SwapChainUsage.ShaderResource | SwapChainUsage.InputAttachment;
			Transform = SwapChainTransform.Optimal;
#if OSX
			// We need at least 3 buffers in Metal to avoid massive
			// performance degradation in full screen mode.
			// https://github.com/KhronosGroup/MoltenVK/issues/808
			BufferCount = 3; 
#else
			BufferCount = 2;
#endif
			DefaultDepthValue = 1f;
			DefaultStencilValue = 0;
			IsPrimary = true;
		}
	}
	
	public interface ISwapChain : IGPUObject
	{
		public SwapChainDesc Desc { get; }
		public void Present(bool vsync);
	}
}
