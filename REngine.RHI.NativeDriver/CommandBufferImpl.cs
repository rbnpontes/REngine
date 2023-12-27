using REngine.Core.Mathematics;
using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class CommandBufferImpl : NativeObject, ICommandBuffer
	{
		private readonly bool pIsDeferred;

		public CommandBufferImpl(IntPtr handle, bool isDeferred) : base(handle)
		{
			pIsDeferred = isDeferred;
		}

		private byte GetIsDeferredByte()
		{
			return (byte)(pIsDeferred ? 1 : 0);
		}

		public ICommandBuffer ClearDepth(ITextureView depthStencil, ClearDepthStencil clearFlags, float depth, byte stencil)
		{
			rengine_cmdbuffer_cleardepth(
				Handle,
				depthStencil.Handle,
				(uint)clearFlags,
				depth,
				stencil,
				GetIsDeferredByte()
			);
			return this;
		}

		public unsafe ICommandBuffer ClearRT(ITextureView renderTarget, in Color clearColor)
		{
			pCopyColor[0] = clearColor.R / 255.0f;
			pCopyColor[1] = clearColor.G / 255.0f;
			pCopyColor[2] = clearColor.B / 255.0f;
			pCopyColor[3] = clearColor.A / 255.0f;

			fixed(float* colorPtr = pCopyColor)
			{
				rengine_cmdbuffer_clearrt(
					Handle,
					renderTarget.Handle,
					new IntPtr(colorPtr),
					GetIsDeferredByte()
				);
			}
			return this;
		}

		public ICommandBuffer CommitBindings(IShaderResourceBinding resourceBinding)
		{
			rengine_cmdbuffer_commitbindings(
				Handle,
				resourceBinding.Handle,
				GetIsDeferredByte()
			);
			return this;
		}

		public unsafe ICommandBuffer Copy(CopyTextureInfo copyInfo)
		{
			Box srcBox = copyInfo.SrcBox ?? new Box();
			CopyTextureInfoDTO.Fill(copyInfo, out CopyTextureInfoDTO output);
			if (copyInfo.SrcBox != null)
				output.srcBox = new IntPtr(Unsafe.AsPointer(ref srcBox));

			rengine_cmdbuffer_copy_tex(Handle, ref output, GetIsDeferredByte());
			return this;
		}

		public ICommandBuffer Draw(DrawArgs args)
		{
			DrawAttribsNative.Fill(args, ref pCopyDrawArgs);
			rengine_cmdbuffer_draw(Handle, ref pCopyDrawArgs);
			return this;
		}

		public ICommandBuffer Draw(DrawIndexedArgs args)
		{
			DrawIndexedAttribsNative.Fill(args, ref pCopyIndexedDrawArgs);
			rengine_cmdbuffer_drawindexed(Handle, ref pCopyIndexedDrawArgs);
			return this;
		}

		public unsafe Span<T> Map<T>(IBuffer buffer, MapType mapType, MapFlags mapFlags) where T : unmanaged
		{
			return MemoryMarshal.Cast<byte, T>(new Span<byte>(
				Map(buffer, mapType, mapFlags).ToPointer(),
				(int)buffer.Size
			));
		}

		public IntPtr Map(IBuffer buffer, MapType mapType, MapFlags mapFlags)
		{
			return rengine_cmdbuffer_map(
				Handle,
				buffer.Handle,
				(byte)mapType,
				(byte)mapFlags
			);
		}

		public ICommandBuffer SetIndexBuffer(IBuffer buffer, ulong byteOffset = 0)
		{
			rengine_cmdbuffer_setibuffer(
				Handle,
				buffer.Handle,
				byteOffset,
				GetIsDeferredByte()
			);
			return this;
		}

		public ICommandBuffer SetPipeline(IPipelineState pipelineState)
		{
			rengine_cmdbuffer_setpipeline(
				Handle,
				pipelineState.Handle
			);
			return this;
		}

		public ICommandBuffer SetPipeline(IComputePipelineState pipelineState)
		{
			rengine_cmdbuffer_setpipeline(
				Handle,
				pipelineState.Handle
			);
			return this;
		}

		public ICommandBuffer SetRT(ITextureView rt, ITextureView depthStencil)
		{
#if DEBUG
			ValidateTextureView(rt);
			ValidateTextureView(depthStencil);
#endif
			pCopyRenderTargetsPointers[0] = rt.Handle;
			InternalSetRTs(1, depthStencil.Handle);
			return this;
		}

		public ICommandBuffer SetRTs(ITextureView[] rts, ITextureView? depthStencil)
		{
			if (rts.Length > MaxRenderTargets)
				throw new ArgumentOutOfRangeException($"Max allowed Render Targets is {MaxRenderTargets}");

			for (var i = 0; i < rts.Length; ++i)
			{
#if DEBUG
				ValidateTextureView(rts[i]);
#endif
				pCopyRenderTargetsPointers[i] = rts[i].Handle;
			}
#if DEBUG
			if(depthStencil != null)
				ValidateTextureView(depthStencil);
#endif

			InternalSetRTs((byte)rts.Length, depthStencil?.Handle ?? IntPtr.Zero);
			return this;
		}

		private unsafe void InternalSetRTs(byte numRts, IntPtr depthStencil)
		{
			fixed (void* rtsPtr = pCopyRenderTargetsPointers)
			{
				rengine_cmdbuffer_setrts(
					Handle,
					new IntPtr(rtsPtr),
					numRts,
					depthStencil,
					GetIsDeferredByte()
				);
			}
		}
#if DEBUG
		private static void ValidateTextureView(ITextureView texView)
		{
			if (texView.IsDisposed)
				throw new ObjectDisposedException($"{nameof(ITextureView)} is Disposed");
			if (texView.Handle == IntPtr.Zero)
				throw new NullReferenceException($"{nameof(ITextureView)} Handle is null");
		}
#endif

		public ICommandBuffer SetVertexBuffer(IBuffer buffer, bool reset)
		{
#if DEBUG
			ValidateVertexBuffer(buffer);
#endif
			pCopyVertexBuffersPointers[0] = buffer.Handle;
			InternalSetVertexBuffers(0, 1, pCopyOffsets, reset);
			return this;
		}

		public ICommandBuffer SetVertexBuffers(uint startSlot, IBuffer[] buffers, bool reset)
		{
#if DEBUG
			ValidateVertexBuffers(buffers);
#endif
			for (var i = 0; i < buffers.Length; ++i)
			{
#if DEBUG
				ValidateVertexBuffer(buffers[i]);
#endif
				pCopyVertexBuffersPointers[i] = buffers[i].Handle;
				pCopyOffsets[i] = ulong.MinValue;
			}

			InternalSetVertexBuffers(startSlot, (uint)buffers.Length, pCopyOffsets, reset);
			return this;
		}

		public ICommandBuffer SetVertexBuffers(uint startSlot, IBuffer[] buffers, ulong[] offsets, bool reset = true)
		{
			var bufferCount = (uint)buffers.Length;
#if DEBUG
			if (offsets.Length != bufferCount)
				throw new ArgumentException("Offsets Length must be the same size of Vertex Buffers");
			ValidateVertexBuffers(buffers);
#endif
			for (var i = 0; i < bufferCount; ++i)
			{
#if DEBUG
				ValidateVertexBuffer(buffers[i]);
#endif
				pCopyVertexBuffersPointers[i] = buffers[i].Handle;
			}

			InternalSetVertexBuffers(startSlot, bufferCount, offsets, reset);
			return this;
		}

#if DEBUG
		private static void ValidateVertexBuffers(IBuffer[] buffers)
		{
			if(buffers.Length > MaxVertexBuffers)
				throw new ArgumentOutOfRangeException($"Vertex Buffer items cannot greater than '{MaxVertexBuffers}'.");
		}

		private static void ValidateVertexBuffer(IBuffer buffer)
		{
			if (buffer.IsDisposed)
				throw new ObjectDisposedException("Vertex Buffer is Disposed");
			if (buffer.Handle == IntPtr.Zero)
				throw new InvalidOperationException("Vertex Buffer Handle is Null");
		}
#endif
		private unsafe void InternalSetVertexBuffers(uint startSlot, uint bufferCount, ulong[] offsets, bool reset)
		{
			fixed (void* ptr = pCopyVertexBuffersPointers)
			{
				fixed (ulong* offsetsPtr = offsets)
				{
					rengine_cmdbuffer_setvbuffer(
						Handle,
						startSlot,
						bufferCount,
						new IntPtr(ptr),
						new IntPtr(offsetsPtr),
						(byte)(reset ? 0x1 : 0x0),
						GetIsDeferredByte()
					);
				}
			}
		}

		public ICommandBuffer Unmap(IBuffer buffer, MapType mapType)
		{
			rengine_cmdbuffer_unmap(Handle, buffer.Handle, (byte)mapType);
			return this;
		}

		public unsafe ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, T data) where T : unmanaged
		{
			return UpdateBuffer(buffer, offset, (ulong)Unsafe.SizeOf<T>(), new IntPtr(Unsafe.AsPointer(ref data)));
		}

		public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, byte[] data)
		{
			return UpdateBuffer(buffer, offset, new ReadOnlySpan<byte>(data));
		}

		public unsafe ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, ReadOnlySpan<T> data) where T : unmanaged
		{
			fixed(T* dataPtr = data)
				UpdateBuffer(buffer, offset, (ulong)(Unsafe.SizeOf<T>() * data.Length), new IntPtr(dataPtr));
			return this;
		}

		public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, ulong size, IntPtr data)
		{
			rengine_cmdbuffer_updtbuffer(
				Handle, 
				buffer.Handle, 
				offset, 
				size, 
				data, 
				GetIsDeferredByte()
			);
			return this;
		}

		public ICommandBuffer SetBlendFactors(in Color color)
		{
			rengine_cmdbuffer_setblendfactors(
				Handle,
				color.R / 255.0f,
				color.G / 255.0f,
				color.B / 255.0f,
				color.A / 255.0f
			);
			return this;
		}

		public unsafe ICommandBuffer SetViewports(Viewport[] viewports, uint rtWidth, uint rtHeight)
		{
			fixed(Viewport* ptr = viewports)
			{
				IntPtr viewportPtr = new(ptr);
				rengine_cmdbuffer_setviewports(
					Handle,
					viewportPtr,
					(byte)viewports.Length,
					rtWidth,
					rtHeight
				);
			}
			return this;
		}

		public ICommandBuffer SetViewport(Viewport viewport, uint rtWidth, uint rtHeight)
		{
			pCopyViewport[0] = viewport;
			return SetViewports(pCopyViewport, rtWidth, rtHeight);
		}

		public ICommandBuffer SetScissors(IntRect[] scissors, uint rtWidth, uint rtHeight)
		{
			InternalSetScissors((byte)scissors.Length, scissors, rtWidth, rtHeight);
			return this;
		}

		public ICommandBuffer SetScissor(IntRect scissor, uint rtWidth, uint rtHeight)
		{
			pCopyScissors[0] = scissor;
			InternalSetScissors(1, pCopyScissors, rtWidth, rtHeight);
			return this;
		}

		private unsafe void InternalSetScissors(byte scissorsCount, IntRect[] scissors, uint rtWidth, uint rtHeight)
		{
			fixed (IntRect* ptr = scissors)
			{
				rengine_cmdbuffer_setscissors(
					Handle,
					new IntPtr(ptr),
					scissorsCount,
					rtWidth,
					rtHeight
				);
			}
		}

		public ICommandBuffer Compute(ComputeArgs args)
		{
			rengine_cmdbuffer_compute(
				Handle,
				ref args
			);
			return this;
		}
#if DEBUG
		public unsafe ICommandBuffer BeginDebugGroup(string name, Color color)
		{
			float[] colorValues = new float[] 
			{
				color.R / 255.0f,
				color.G / 255.0f,
				color.B / 255.0f,
				color.A / 255.0f
			};
			IntPtr namePtr = Marshal.StringToHGlobalAnsi(name);
			fixed(float* colorPtr = colorValues)
			{
				rengine_cmdbuffer_begin_dbg_grp(
					Handle,
					namePtr,
					new IntPtr(colorPtr)
				);
			}

			Marshal.FreeHGlobal(namePtr);
			return this;
		}

		public ICommandBuffer EndDebugGroup()
		{
			rengine_cmdbuffer_end_dbg_grp(Handle);
			return this;
		}

		public unsafe ICommandBuffer InsertDebugLabel(string label, Color color)
		{
			float[] colorValues = new float[]
			{
				color.R / 255.0f,
				color.G / 255.0f,
				color.B / 255.0f,
				color.A / 255.0f
			};
			IntPtr labelPtr = Marshal.StringToHGlobalAnsi(label);
			fixed(float* colorPtr = colorValues)
			{
				rengine_cmdbuffer_insert_dbg_label(
					Handle,
					labelPtr,
					new IntPtr(colorPtr)
				);
			}
			Marshal.FreeHGlobal(labelPtr);
			return this;
		}
#endif
		public ICommandBuffer Begin(uint immediateContextId)
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
			rengine_cmdbuffer_begin(Handle, immediateContextId);
			return this;
		}

		public ICommandBuffer FinishFrame()
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
			rengine_cmdbuffer_finish_frame(Handle);
			return this;
		}

		public ICommandBuffer FinishCommandList(out ICommandList commandList)
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
			var list = rengine_cmdbuffer_finish_command_list(Handle);
			if (list == IntPtr.Zero)
				throw new NullReferenceException("Could not possible to finish command list.");
			commandList= new CommandListImpl(list);
			return this;
		}

		public ICommandBuffer TransitionShaderResource(IPipelineState pipelineState, IShaderResourceBinding binding)
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
			rengine_cmdbuffer_transition_shader_resources(
				Handle,
				pipelineState.Handle,
				binding.Handle);
			return this;
		}

		public ICommandBuffer SetStencilRef(uint stencilRef)
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
			rengine_cmdbuffer_set_stencil_ref(Handle, stencilRef);
			return this;
		}

		public ICommandBuffer InvalidateState()
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
			rengine_cmdbuffer_invalidate_state(Handle);
			return this;
		}

		public ICommandBuffer NextSubpass()
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
			rengine_cmdbuffer_next_subpass(Handle);
			return this;
		}

		public ICommandBuffer GenerateMips(ITextureView textureView)
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
			rengine_cmdbuffer_generate_mips(Handle, textureView.Handle);
			return this;
		}

		public unsafe ICommandBuffer TransitionResourceStates(StateTransitionDesc[] resourceBarriers)
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
			var data = pBarriersPool.Rent(resourceBarriers.Length);
			for (var i = 0; i < resourceBarriers.Length; ++i)
			{
				StateTransitionDTO.Fill(resourceBarriers[i], out var desc);
				data[i] = desc;
			}

			ReadOnlySpan<StateTransitionDTO> dataSpan = new(data);
			fixed (StateTransitionDTO* dataPtr = dataSpan)
			{
				rengine_cmdbuffer_transition_resource_states(
					Handle, 
					(uint)resourceBarriers.Length, 
					new IntPtr(dataPtr));
			}
			
			pBarriersPool.Return(data);
			return this;
		}

		public ICommandBuffer ResolveTextureSubresource(
			ITexture srcTexture,
			ITexture dstTexture,
			ResolveTextureSubresourceDesc resolveDesc)
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
			ResolveTextureSubresourceDTO.Fill(resolveDesc, out var desc);
			
			rengine_cmdbuffer_resolve_texture_subresource(
				Handle,
				srcTexture.Handle,
				dstTexture.Handle,
				ref desc);
			return this;
		}
	}

}
