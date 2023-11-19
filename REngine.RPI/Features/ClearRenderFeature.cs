using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.RHI;

namespace REngine.RPI.Features
{
	public sealed class ClearRenderFeature : IRenderFeature
	{
		private RenderState? pRenderState;
		private IRenderer? pRenderer;

		public bool IsDirty { get; private set; } = true;
		public bool IsDisposed { get; } = false;

		public Color ClearColor { get; set; } = Color.Black;
		public float ClearDepthValue { get; set; } = 1f;
		public byte ClearStencilValue { get; set; } = 0;
		public ITextureView? ColorBuffer { get; set; }
		public ITextureView? DepthBuffer { get; set; }
		public void Dispose()
		{
		}

		public IRenderFeature MarkAsDirty() => this;
		public IRenderFeature Setup(in RenderFeatureSetupInfo execInfo)
		{
			pRenderState = execInfo.RenderState;
			pRenderer = execInfo.Renderer;
			IsDirty = false;
			return this;
		}
		public IRenderFeature Compile(ICommandBuffer command) => this;

		public IRenderFeature Execute(ICommandBuffer command)
		{
			var swapChain = pRenderer?.SwapChain;
			if (pRenderState is null || pRenderer is null || swapChain is null)
				return this;

			var rts = Array.Empty<ITextureView>();
			Viewport viewport = new();

			var colorBufferSize = ColorBuffer?.Parent.Desc.Size ?? new TextureSize();
			var depthBufferSize = DepthBuffer?.Parent.Desc.Size ?? new TextureSize();

			if (ColorBuffer is not null)
			{
				rts = new [] { ColorBuffer };
				viewport.Size = new Vector2(colorBufferSize.Width, colorBufferSize.Height);
			}

			if (colorBufferSize.Width != depthBufferSize.Width || colorBufferSize.Height != depthBufferSize.Height)
				throw new InvalidOperationException("Color Buffer and Depth Buffer must have the same size");

			command
				.SetRTs(rts, DepthBuffer)
				.SetViewport(viewport, colorBufferSize.Width, colorBufferSize.Height)
				.ClearRT(ColorBuffer, ClearColor)
				.ClearDepth(
					DepthBuffer,
					ClearDepthStencil.Depth,
					ClearDepthValue,
					ClearStencilValue
				);

			var swapChainSize = swapChain.Size;

			command
				.SetRT(swapChain.ColorBuffer, swapChain.DepthBuffer)
				.SetViewport(pRenderState.Viewport, swapChainSize.Width, swapChainSize.Height);
			return this;
		}
	}
}
