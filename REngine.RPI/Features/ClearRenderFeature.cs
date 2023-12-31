﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.IO;
using REngine.RHI;

namespace REngine.RPI.Features
{
	public sealed class ClearRenderFeature : IRenderFeature
	{
		private RenderState? pRenderState;
		private IRenderer? pRenderer;

		public bool IsDirty { get; private set; } = true;
		public bool IsDisposed { get; } = false;

		public Color? ClearColor { get; set; }
		public float? ClearDepthValue { get; set; }
		public byte? ClearStencilValue { get; set; }
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
#if PROFILER
			using var _ = Profiler.Instance.Begin();
#endif
			var swapChain = pRenderer?.SwapChain;
			if (pRenderState is null || pRenderer is null || swapChain is null)
				return this;

			Viewport viewport = new();

			var colorBufferSize = ColorBuffer?.Parent.Desc.Size ?? new TextureSize();

			if (ColorBuffer is not null)
				viewport.Size = new Vector2(colorBufferSize.Width, colorBufferSize.Height);

			command
				.SetRT(ColorBuffer, DepthBuffer)
				.SetViewport(viewport, colorBufferSize.Width, colorBufferSize.Height)
				.ClearRT(ColorBuffer, ClearColor ?? pRenderState.DefaultClearColor);
			if (DepthBuffer != null)
			{
				command
					.ClearDepth(
						DepthBuffer,
						ClearDepthStencil.Depth,
						ClearDepthValue ?? pRenderState.DefaultClearDepthValue,
						ClearStencilValue ?? pRenderState.DefaultClearStencilValue
					);
			}

			var swapChainSize = swapChain.Size;

			command
				.SetRT(swapChain.ColorBuffer, swapChain.DepthBuffer)
				.SetViewport(pRenderState.Viewport, swapChainSize.Width, swapChainSize.Height);
			return this;
		}
	}
}
