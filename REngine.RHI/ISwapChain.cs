using System;
using System.Collections.Generic;
using System.Drawing;
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
		public SwapChainSize(Size size)
		{
			Width = (uint)size.Width;
			Height = (uint)size.Height;
		}
	}
	
	public struct SwapChainFormats
	{
		public TextureFormat Color;
		public TextureFormat Depth;

		public SwapChainFormats()
		{
			Color = Depth = TextureFormat.Unknown;
		}
		public SwapChainFormats(TextureFormat color, TextureFormat depth = TextureFormat.D32Float)
		{
			Color = color;
			Depth = depth;
		}
		public SwapChainFormats(GraphicsSettings settings)
		{
			Color = settings.DefaultColorFormat;
			Depth = settings.DefaultDepthFormat;
		}

		public static readonly SwapChainFormats None = new SwapChainFormats();
		public static readonly SwapChainFormats RGBA = new SwapChainFormats(TextureFormat.RGBA8UNorm);
		public static readonly SwapChainFormats RGBASrgb = new SwapChainFormats(TextureFormat.RGBA8UNormSRGB);
		public static readonly SwapChainFormats BGRA = new SwapChainFormats(TextureFormat.BGRA8UNorm);
		public static readonly SwapChainFormats BGRASrgb = new SwapChainFormats(TextureFormat.BGRA8UNormSRGB);
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

		public SwapChainDesc(GraphicsSettings settings)
		{
			Size = new SwapChainSize();
			Formats = new SwapChainFormats(settings);
			BufferCount = settings.DefaultSwapChainBufferCount;
			Usage = SwapChainUsage.RenderTarget | SwapChainUsage.ShaderResource | SwapChainUsage.ShaderResource;
			Transform = SwapChainTransform.Optimal;
			DefaultDepthValue = 1f;
			DefaultStencilValue = 0;
			IsPrimary = true;
		}
	}
	
	public class SwapChainResizeEventArgs : EventArgs
	{
		public SwapChainSize Size { get; private set; }
		public SwapChainTransform Transform { get; private set; }
		public SwapChainResizeEventArgs(SwapChainSize size, SwapChainTransform transform)
		{
			Size = size;
			Transform = transform;
		}
	}
	public interface ISwapChain : IDisposable
	{
		public event EventHandler<SwapChainResizeEventArgs>? OnResize;
		public event EventHandler? OnPresent;
		public event EventHandler? OnDispose;

		public SwapChainDesc Desc { get; }
		public SwapChainSize Size { get; set; }
		public SwapChainTransform Transform { get; set; }
		public ITextureView ColorBuffer { get; }
		public ITextureView? DepthBuffer { get; }
		public uint BufferCount { get; }

		public ISwapChain Present(bool vsync);
		public ISwapChain Resize(Size size, SwapChainTransform transform = SwapChainTransform.Optimal);
		public ISwapChain Resize(uint width, uint height, SwapChainTransform transform = SwapChainTransform.Optimal);
	}
}
