using Diligent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver.Adapters
{
	internal static class SwapChainAdapter
	{
		public static void ConvertFromDiligent(ref Diligent.SwapChainDesc desc, out RHI.SwapChainDesc outDesc)
		{
			outDesc = new RHI.SwapChainDesc
			{
				Size = new SwapChainSize { Width = desc.Width, Height = desc.Height },
				Formats = new SwapChainFormats { Color = (RHI.TextureFormat)desc.ColorBufferFormat, Depth = (RHI.TextureFormat)desc.DepthBufferFormat },
				Transform = (SwapChainTransform)desc.PreTransform,
				BufferCount = desc.BufferCount,
				DefaultDepthValue = desc.DefaultDepthValue,
				DefaultStencilValue = desc.DefaultStencilValue,
				IsPrimary = desc.IsPrimary,
			};
		}
		public static void ConvertToDiligent(ref RHI.SwapChainDesc desc, out Diligent.SwapChainDesc outDesc)
		{
			outDesc = new Diligent.SwapChainDesc
			{
				Width = desc.Size.Width,
				Height = desc.Size.Height,
				ColorBufferFormat = (Diligent.TextureFormat)desc.Formats.Color,
				DepthBufferFormat = (Diligent.TextureFormat)desc.Formats.Depth,
				PreTransform = (SurfaceTransform)desc.Transform,
				BufferCount = desc.BufferCount,
				DefaultDepthValue = desc.DefaultDepthValue,
				DefaultStencilValue = desc.DefaultStencilValue,
				IsPrimary = desc.IsPrimary,
			};
		}
	}
}
