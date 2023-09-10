using Diligent;
using REngine.RHI.DiligentDriver.Adapters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class CommandBufferImpl : ICommandBuffer, INativeObject
	{
		private IDeviceContext? pCtx;
		private bool pIsDeferred;
		private DrawAttribsAdapter pDrawAdapter = new DrawAttribsAdapter();

		public object? Handle => pCtx;
		public bool IsDisposed => pCtx == null;
		
		public CommandBufferImpl(IDeviceContext deviceContext, bool isDeferred)
		{
			pCtx = deviceContext;
			pIsDeferred = isDeferred;
		}


		public ICommandBuffer ClearDepth(ITextureView depthStencil, ClearDepthStencil clearFlags, float depth, byte stencil)
		{
			if (pCtx is null)
				throw new NullReferenceException("Can´t clear depth, command buffer has been already disposed.");

			pCtx.ClearDepthStencil(
				NativeObjectUtils.Get<Diligent.ITextureView>(depthStencil),
				(ClearDepthStencilFlags)clearFlags,
				depth,
				stencil,
				pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition
			);
			return this;
		}

		public ICommandBuffer ClearRT(ITextureView renderTarget, in Color clearColor)
		{
			if (pCtx is null)
				throw new NullReferenceException("Can´t clear rt, command buffer has been already disposed.");

			pCtx.ClearRenderTarget(
				NativeObjectUtils.Get<Diligent.ITextureView>(renderTarget),
				new Vector4(
					clearColor.R / 255.0f, 
					clearColor.G / 255.0f, 
					clearColor.B / 255.0f, 
					clearColor.A / 255.0f
				),
				pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition
			);
			return this;
		}

		public void Dispose()
		{
			pCtx?.Dispose();
			pCtx = null;
		}

		public ICommandBuffer Draw(DrawArgs args)
		{
			if (pCtx is null)
				throw new NullReferenceException("Can´t execute draw, command buffer has been already disposed.");

			DrawAttribs drawAttrs;
			pDrawAdapter.Fill(ref args, out drawAttrs);

			pCtx.Draw(drawAttrs);
			return this;
		}

		public ICommandBuffer SetPipeline(IPipelineState pipelineState)
		{
			if (pCtx is null)
				throw new NullReferenceException("Can´t set pipeline state, command buffer has been already disposed.");

			pCtx.SetPipelineState(NativeObjectUtils.Get<Diligent.IPipelineState>(pipelineState));
			return this;
		}

		public ICommandBuffer SetPipeline(IComputePipelineState pipelineState)
		{
			if (pCtx is null)
				throw new NullReferenceException("Can´t set pipeline state, command buffer has been already disposed.");
			pCtx.SetPipelineState(NativeObjectUtils.Get<Diligent.IPipelineState>(pipelineState));
			return this;
		}

		public ICommandBuffer SetRTs(ITextureView[] rts, ITextureView depthStencil)
		{
			if (pCtx is null)
				throw new NullReferenceException("Can´t set render targets, command buffer has been already disposed.");
			// TODO: optimize
			Diligent.ITextureView[] textures = new Diligent.ITextureView[rts.Length];
			Parallel.For(0, rts.Length, i =>
			{
				textures[i] = NativeObjectUtils.Get<Diligent.ITextureView>(rts[i]);
			});
			pCtx.SetRenderTargets(textures, NativeObjectUtils.Get<Diligent.ITextureView>(depthStencil), pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition);
			return this;
		}
	}
}
