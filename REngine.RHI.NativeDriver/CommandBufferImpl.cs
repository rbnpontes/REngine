using REngine.Core.Mathematics;
using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.IO;

namespace REngine.RHI.NativeDriver
{
    internal partial class CommandBufferImpl(IntPtr handle, bool isDeferred) : NativeObject(handle), ICommandBuffer
    {
        private byte GetIsDeferredByte()
        {
            return (byte)(isDeferred ? 1 : 0);
        }

        public ICommandBuffer ClearDepth(ITextureView depthStencil, ClearDepthStencil clearFlags, float depth,
            byte stencil)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(depthStencil);
#endif
#if PROFILER
            using (Profiler.Instance.Begin(ClearDepthName))
            {
#endif
            rengine_cmdbuffer_cleardepth(
                Handle,
                depthStencil.Handle,
                (uint)clearFlags,
                depth,
                stencil,
                GetIsDeferredByte()
            );
#if PROFILER
            }
#endif
            return this;
        }

        public unsafe ICommandBuffer ClearRT(ITextureView renderTarget, in Color clearColor)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(renderTarget);
#endif
            pCopyColor[0] = clearColor.R / 255.0f;
            pCopyColor[1] = clearColor.G / 255.0f;
            pCopyColor[2] = clearColor.B / 255.0f;
            pCopyColor[3] = clearColor.A / 255.0f;

            fixed (float* colorPtr = pCopyColor)
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
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(resourceBinding);
#endif
            rengine_cmdbuffer_commitbindings(
                Handle,
                resourceBinding.Handle,
                GetIsDeferredByte()
            );
            return this;
        }

        public unsafe ICommandBuffer Copy(CopyTextureInfo copyInfo)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (copyInfo.SrcTexture is not null)
                ValidateGpuObject(copyInfo.SrcTexture);
            if (copyInfo.DstTexture is not null)
                ValidateGpuObject(copyInfo.DstTexture);
#endif
            Box srcBox = copyInfo.SrcBox ?? new Box();
            CopyTextureInfoDTO.Fill(copyInfo, out CopyTextureInfoDTO output);
            if (copyInfo.SrcBox != null)
                output.srcBox = new IntPtr(Unsafe.AsPointer(ref srcBox));

            rengine_cmdbuffer_copy_tex(Handle, ref output, GetIsDeferredByte());
            return this;
        }

        public ICommandBuffer Draw(DrawArgs args)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            DrawAttribsNative.Fill(args, ref pCopyDrawArgs);
            rengine_cmdbuffer_draw(Handle, ref pCopyDrawArgs);
            return this;
        }

        public ICommandBuffer Draw(DrawIndexedArgs args)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            DrawIndexedAttribsNative.Fill(args, ref pCopyIndexedDrawArgs);
            rengine_cmdbuffer_drawindexed(Handle, ref pCopyIndexedDrawArgs);
            return this;
        }

        public unsafe Span<T> Map<T>(IBuffer buffer, MapType mapType, MapFlags mapFlags) where T : unmanaged
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(buffer);
#endif
            return MemoryMarshal.Cast<byte, T>(new Span<byte>(
                Map(buffer, mapType, mapFlags).ToPointer(),
                (int)buffer.Size
            ));
        }

        public IntPtr Map(IBuffer buffer, MapType mapType, MapFlags mapFlags)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(buffer);
#endif
            return rengine_cmdbuffer_map(
                Handle,
                buffer.Handle,
                (byte)mapType,
                (byte)mapFlags
            );
        }

        public ICommandBuffer SetIndexBuffer(IBuffer buffer, ulong byteOffset = 0)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(buffer);
#endif
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
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(pipelineState);
#endif
            rengine_cmdbuffer_setpipeline(
                Handle,
                pipelineState.Handle
            );
            return this;
        }

        public ICommandBuffer SetPipeline(IComputePipelineState pipelineState)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(pipelineState);
#endif
            rengine_cmdbuffer_setpipeline(
                Handle,
                pipelineState.Handle
            );
            return this;
        }

        public ICommandBuffer SetRT(ITextureView rt, ITextureView? depthStencil)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(rt);
            if (depthStencil is not null)
                ValidateGpuObject(depthStencil);
#endif
            pCopyRenderTargetsPointers[0] = rt.Handle;
            InternalSetRTs(1, depthStencil?.Handle ?? IntPtr.Zero);
            return this;
        }

        public ICommandBuffer SetRTs(ITextureView[] rts, ITextureView? depthStencil)
        {
#if RENGINE_VALIDATIONS
            if (rts.Length > MaxRenderTargets)
                throw new ArgumentOutOfRangeException($"Max allowed Render Targets is {MaxRenderTargets}");
#endif

            for (var i = 0; i < rts.Length; ++i)
            {
#if RENGINE_VALIDATIONS
                ValidateGpuObject(rts[i]);
#endif
                pCopyRenderTargetsPointers[i] = rts[i].Handle;
            }
#if RENGINE_VALIDATIONS
            if (depthStencil != null)
                ValidateGpuObject(depthStencil);
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
#if RENGINE_VALIDATIONS
        private static void ValidateGpuObject(INativeObject texView)
        {
            if (texView.IsDisposed)
                throw new ObjectDisposedException($"{nameof(ITextureView)} is Disposed");
            if (texView.Handle == IntPtr.Zero)
                throw new NullReferenceException($"{nameof(ITextureView)} Handle is null");
        }
#endif

        public ICommandBuffer SetVertexBuffer(IBuffer buffer, bool reset)
        {
#if RENGINE_VALIDATIONS
            ValidateGpuObject(buffer);
#endif
            pCopyVertexBuffersPointers[0] = buffer.Handle;
            InternalSetVertexBuffers(0, 1, pCopyOffsets, reset);
            return this;
        }

        public ICommandBuffer SetVertexBuffers(uint startSlot, IBuffer[] buffers, bool reset)
        {
#if RENGINE_VALIDATIONS
            ValidateVertexBuffers(buffers);
#endif
            for (var i = 0; i < buffers.Length; ++i)
            {
#if RENGINE_VALIDATIONS
                ValidateGpuObject(buffers[i]);
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
#if RENGINE_VALIDATIONS
            if (offsets.Length != bufferCount)
                throw new ArgumentException("Offsets Length must be the same size of Vertex Buffers");
            ValidateVertexBuffers(buffers);
#endif
            for (var i = 0; i < bufferCount; ++i)
            {
#if RENGINE_VALIDATIONS
                ValidateGpuObject(buffers[i]);
#endif
                pCopyVertexBuffersPointers[i] = buffers[i].Handle;
            }

            InternalSetVertexBuffers(startSlot, bufferCount, offsets, reset);
            return this;
        }

#if RENGINE_VALIDATIONS
        private static void ValidateVertexBuffers(IReadOnlyCollection<IBuffer> buffers)
        {
            if (buffers.Count > MaxVertexBuffers)
                throw new ArgumentOutOfRangeException($"Vertex Buffer items cannot greater than '{MaxVertexBuffers}'.");
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
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(buffer);
#endif
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

        public unsafe ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, Span<T> data) where T : unmanaged
        {
            fixed (T* dataPtr = data)
                UpdateBuffer(buffer, offset, (ulong)(Unsafe.SizeOf<T>() * data.Length), new IntPtr(dataPtr));
            return this;
        }

        public unsafe ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, ReadOnlySpan<T> data)
            where T : unmanaged
        {
            fixed (T* dataPtr = data)
                UpdateBuffer(buffer, offset, (ulong)(Unsafe.SizeOf<T>() * data.Length), new IntPtr(dataPtr));
            return this;
        }

        public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, ulong size, IntPtr data)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(buffer);
#endif
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
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
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
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            fixed (Viewport* ptr = viewports)
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
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            pCopyViewport[0] = viewport;
            return SetViewports(pCopyViewport, rtWidth, rtHeight);
        }

        public ICommandBuffer SetScissors(IntRect[] scissors, uint rtWidth, uint rtHeight)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            InternalSetScissors((byte)scissors.Length, scissors, rtWidth, rtHeight);
            return this;
        }

        public ICommandBuffer SetScissor(IntRect scissor, uint rtWidth, uint rtHeight)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
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
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            var colorValues = new float[]
            {
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f,
                color.A / 255.0f
            };
            var namePtr = Marshal.StringToHGlobalAnsi(name);
            fixed (float* colorPtr = colorValues)
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
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            rengine_cmdbuffer_end_dbg_grp(Handle);
            return this;
        }

        public unsafe ICommandBuffer InsertDebugLabel(string label, Color color)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            var colorValues = new float[]
            {
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f,
                color.A / 255.0f
            };
            var labelPtr = Marshal.StringToHGlobalAnsi(label);
            fixed (float* colorPtr = colorValues)
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
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            if (!isDeferred)
                return this;
            rengine_cmdbuffer_begin(Handle, immediateContextId);
            return this;
        }

        public ICommandBuffer FinishFrame()
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            if (!isDeferred)
                return this;
            rengine_cmdbuffer_finish_frame(Handle);
            return this;
        }

        public ICommandBuffer FinishCommandList(out ICommandList commandList)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            var list = rengine_cmdbuffer_finish_command_list(Handle);
            if (list == IntPtr.Zero)
                throw new NullReferenceException("Could not possible to finish command list.");
            commandList = new CommandListImpl(list);
            return this;
        }

        public ICommandBuffer TransitionShaderResource(IPipelineState pipelineState, IShaderResourceBinding binding)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(pipelineState);
            ValidateGpuObject(binding);
#endif
            rengine_cmdbuffer_transition_shader_resources(
                Handle,
                pipelineState.Handle,
                binding.Handle);
            return this;
        }

        public ICommandBuffer SetStencilRef(uint stencilRef)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            rengine_cmdbuffer_set_stencil_ref(Handle, stencilRef);
            return this;
        }

        public ICommandBuffer InvalidateState()
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            rengine_cmdbuffer_invalidate_state(Handle);
            return this;
        }

        public ICommandBuffer NextSubpass()
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            rengine_cmdbuffer_next_subpass(Handle);
            return this;
        }

        public ICommandBuffer GenerateMips(ITextureView textureView)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateGpuObject(textureView);
#endif
            rengine_cmdbuffer_generate_mips(Handle, textureView.Handle);
            return this;
        }

        public unsafe ICommandBuffer TransitionResourceStates(StateTransitionDesc[] resourceBarriers)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            var data = pBarriersPool.Rent(resourceBarriers.Length);
            for (var i = 0; i < resourceBarriers.Length; ++i)
            {
#if RENGINE_VALIDATIONS
                ValidateGpuObject(resourceBarriers[i].Resource);
                if (resourceBarriers[i].ResourceBefore is not null)
                    ValidateGpuObject(resourceBarriers[i].ResourceBefore);
#endif
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
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            ResolveTextureSubresourceDTO.Fill(resolveDesc, out var desc);

            rengine_cmdbuffer_resolve_texture_subresource(
                Handle,
                srcTexture.Handle,
                dstTexture.Handle,
                ref desc);
            return this;
        }

        public unsafe ICommandBuffer ExecuteCommandList(ICommandList[] list)
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            var arr = pPtrsPool.Rent(list.Length);
            for (var i = 0; i < list.Length; ++i)
            {
#if RENGINE_VALIDATIONS
                ValidateGpuObject(list[i]);
#endif
                arr[i] = list[i].Handle;
            }

            var commandListSpan = new ReadOnlySpan<IntPtr>(arr);
            fixed (IntPtr* ptr = commandListSpan)
                rengine_cmdbuffer_exec_command_list(Handle, (uint)list.Length, new IntPtr(ptr));
            pPtrsPool.Return(arr);
            return this;
        }
    }
}