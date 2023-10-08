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
			float[] color = new float[4] { clearColor.R / 255.0f, clearColor.G / 255.0f, clearColor.B / 255.0f, clearColor.A / 255.0f  };
			fixed(float* colorPtr = color)
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
			DrawAttribsNative.Fill(args, out DrawAttribsNative output);
			rengine_cmdbuffer_draw(Handle, ref output);
			return this;
		}

		public ICommandBuffer Draw(DrawIndexedArgs args)
		{
			DrawIndexedAttribsNative.Fill(args, out DrawIndexedAttribsNative output);
			rengine_cmdbuffer_drawindexed(Handle, ref output);
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
			return SetRTs(new ITextureView[] { rt }, depthStencil );
		}

		public unsafe ICommandBuffer SetRTs(ITextureView[] rts, ITextureView depthStencil)
		{
			IntPtr[] renderTargets = rts.Select(x => x.Handle).ToArray();
			fixed(void* rtsPtr = renderTargets)
			{
				rengine_cmdbuffer_setrts(
					Handle,
					new IntPtr(rtsPtr),
					(byte)rts.Length,
					depthStencil.Handle,
					GetIsDeferredByte()
				);
			}

			return this;
		}

		public ICommandBuffer SetVertexBuffer(IBuffer buffer, bool reset)
		{
			return SetVertexBuffers(0, new IBuffer[] { buffer }, new ulong[0], reset);
		}

		public ICommandBuffer SetVertexBuffers(uint startSlot, IEnumerable<IBuffer> buffers, bool reset)
		{
			return SetVertexBuffers(startSlot, buffers, buffers.Select(x => ulong.MinValue).ToArray(), reset);
		}

		public unsafe ICommandBuffer SetVertexBuffers(uint startSlot, IEnumerable<IBuffer> buffers, ulong[] offsets, bool reset = true)
		{
			IntPtr[] buffersPtr = buffers.Select(x => x.Handle).ToArray();
			fixed(void* ptr = buffersPtr)
			{
				fixed(ulong* offsetsPtr = offsets)
				{
					rengine_cmdbuffer_setvbuffer(
						Handle,
						startSlot,
						(uint)buffers.Count(),
						new IntPtr(ptr),
						new IntPtr(offsetsPtr),
						(byte)(reset ? 1 : 0),
						GetIsDeferredByte()
					);
				}
			}
			return this;
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
			return SetViewports(new Viewport[] { viewport }, rtWidth, rtHeight);
		}

		public unsafe ICommandBuffer SetScissors(IntRect[] scissors, uint rtWidth, uint rtHeight)
		{
			fixed(IntRect* ptr = scissors)
			{
				IntPtr scissorsPtr = new(ptr);
				rengine_cmdbuffer_setscissors(
					Handle,
					scissorsPtr, 
					(byte)scissors.Length, 
					rtWidth, 
					rtHeight 
				);
			}

			return this;
		}

		public ICommandBuffer SetScissor(IntRect scissor, uint rtWidth, uint rtHeight)
		{
			return SetScissors(new IntRect[] { scissor }, rtWidth, rtHeight);
		}
	}

}
