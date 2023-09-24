using Diligent;
using REngine.RHI.DiligentDriver.Adapters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
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

		private string SetDisposeMsgError(string resource)
		{
			return $"Can´t set {resource}. ICommandBuffer has been disposed.";
		}
		private string ExecDisposeMsgError(string resource)
		{
			return $"Can´t execute {resource}. ICommandBuffer has been disposed.";
		}

		public ICommandBuffer ClearDepth(ITextureView depthStencil, ClearDepthStencil clearFlags, float depth, byte stencil)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError("ClearDepth"));

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
				throw new ObjectDisposedException(ExecDisposeMsgError("ClearRT"));

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
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(Draw)));

			DrawAttribs drawAttrs;
			pDrawAdapter.Fill(ref args, out drawAttrs);

			pCtx.Draw(drawAttrs);
			return this;
		}
		public ICommandBuffer Draw(DrawIndexedArgs args)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(Draw)));

			DrawIndexedAttribs drawAttrs;
			pDrawAdapter.Fill(ref args, out drawAttrs);

			pCtx.DrawIndexed(drawAttrs);
			return this;
		}

		public ICommandBuffer SetPipeline(IPipelineState pipelineState)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(SetDisposeMsgError("pipeline"));

			pCtx.SetPipelineState(NativeObjectUtils.Get<Diligent.IPipelineState>(pipelineState));
			return this;
		}

		public ICommandBuffer SetPipeline(IComputePipelineState pipelineState)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(SetDisposeMsgError("pipeline"));
			pCtx.SetPipelineState(NativeObjectUtils.Get<Diligent.IPipelineState>(pipelineState));
			return this;
		}

		public ICommandBuffer SetRTs(ITextureView[] rts, ITextureView depthStencil)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(SetDisposeMsgError("render targets"));
			// TODO: optimize
			Diligent.ITextureView[] textures = new Diligent.ITextureView[rts.Length];
			for (int i = 0; i < rts.Length; ++i)
				textures[i] = NativeObjectUtils.Get<Diligent.ITextureView>(rts[i]);
			pCtx.SetRenderTargets(textures, NativeObjectUtils.Get<Diligent.ITextureView>(depthStencil), pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition);
			return this;
		}

		public Span<T> Map<T>(IBuffer buffer, MapType mapType, MapFlags mapFlags) where T : unmanaged
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(Map)));
			return pCtx.MapBuffer<T>(NativeObjectUtils.Get<Diligent.IBuffer>(buffer), (Diligent.MapType)mapType, (Diligent.MapFlags)mapFlags);
		}

		public IntPtr Map(IBuffer buffer, MapType mapType, MapFlags mapFlags)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(Map)));
			return pCtx.MapBuffer(NativeObjectUtils.Get<Diligent.IBuffer>(buffer), (Diligent.MapType)mapType, (Diligent.MapFlags)mapFlags);
		}

		public ICommandBuffer Unmap(IBuffer buffer, MapType mapType)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(Unmap)));
			pCtx.UnmapBuffer(NativeObjectUtils.Get<Diligent.IBuffer>(buffer), (Diligent.MapType)mapType);
			return this;
		}

		public ICommandBuffer CommitBindings(IShaderResourceBinding resourceBinding)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(CommitBindings)));
			pCtx.CommitShaderResources(
				NativeObjectUtils.Get<Diligent.IShaderResourceBinding>(resourceBinding),
				pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition
			);
			return this;
		}

		public ICommandBuffer SetVertexBuffer(IBuffer buffer)
		{
			return SetVertexBuffers(0, new IBuffer[] { buffer });
		}

		public ICommandBuffer SetVertexBuffers(uint startSlot, IEnumerable<IBuffer> buffers)
		{
			ulong[] offsets = new ulong[buffers.Count()];
			Array.Fill<ulong>(offsets, 0);
			return SetVertexBuffers(startSlot, buffers, offsets);
		}

		public ICommandBuffer SetVertexBuffers(uint startSlot, IEnumerable<IBuffer> buffers, ulong[] offsets)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(SetVertexBuffers)));
			Diligent.IBuffer[] nativeBuffers = new Diligent.IBuffer[buffers.Count()];
			for (int i = 0; i < buffers.Count(); ++i)
				nativeBuffers[i] = NativeObjectUtils.Get<Diligent.IBuffer>(buffers.ElementAt(i));

			pCtx.SetVertexBuffers(
				startSlot,
				nativeBuffers,
				offsets,
				pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition
			);
			return this;
		}

		public ICommandBuffer SetIndexBuffer(IBuffer buffer, ulong byteOffset = 0)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(SetIndexBuffer)));

			pCtx.SetIndexBuffer(
				NativeObjectUtils.Get<Diligent.IBuffer>(buffer),
				byteOffset,
				pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition
			);
			return this;
		}

		public ICommandBuffer Copy(CopyTextureInfo copyInfo)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(Copy)));
			Diligent.CopyTextureAttribs attribs;
			new CopyAdapter().Fill(copyInfo, out attribs);
			attribs.SrcTextureTransitionMode =
				attribs.DstTextureTransitionMode = pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition;

			pCtx.CopyTexture(attribs);
			return this;
		}

		public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, T data) where T : unmanaged
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(UpdateBuffer)));

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			pCtx.UpdateBuffer(
				NativeObjectUtils.Get<Diligent.IBuffer>(buffer),
				offset,
				(ulong)Marshal.SizeOf<T>(),
				handle.AddrOfPinnedObject(),
				pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition
			);
			return this;
		}

		public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, byte[] data)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(UpdateBuffer)));

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			pCtx.UpdateBuffer(
				NativeObjectUtils.Get<Diligent.IBuffer>(buffer),
				offset,
				(ulong)(Marshal.SizeOf<byte>() * data.Length),
				handle.AddrOfPinnedObject(),
				pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition
			);
			handle.Free();
			return this;
		}

		public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, ReadOnlySpan<T> data) where T : unmanaged
		{
			if(pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(UpdateBuffer)));

			pCtx.UpdateBuffer(
				NativeObjectUtils.Get<Diligent.IBuffer>(buffer), 
				offset, 
				data, 
				pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition);
			return this;
		}

		public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, ulong size, IntPtr data)
		{
			if (pCtx is null)
				throw new ObjectDisposedException(ExecDisposeMsgError(nameof(UpdateBuffer)));

			pCtx.UpdateBuffer(
				NativeObjectUtils.Get<Diligent.IBuffer>(buffer),
				offset,
				size,
				data,
				pIsDeferred ? ResourceStateTransitionMode.Verify : ResourceStateTransitionMode.Transition
			);
			return this;
		}
	}
}
