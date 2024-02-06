using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using REngine.Core;
using REngine.Core.Mathematics;
using REngine.RHI.Web.Driver.Models;

namespace REngine.RHI.Web.Driver;

internal partial class CommandBufferImpl(IntPtr handle) : NativeObject(handle), ICommandBuffer
{
    private const int MaxVertexBuffers = 4;
    protected override void OnBeginDispose()
    {
        NativeApis.js_free(pTmpMem);
    }
#if RENGINE_VALIDATIONS
    private static void ValidateGpuObject(INativeObject texView)
    {
        ObjectDisposedException.ThrowIf(texView.IsDisposed, texView);
        if (texView.Handle == IntPtr.Zero)
            throw new NullReferenceException($"{nameof(ITextureView)} Handle is null");
    }

    private static void ValidateVertexBuffer(IReadOnlyCollection<IBuffer> buffers)
    {
        if (buffers.Count > MaxVertexBuffers)
            throw new ArgumentOutOfRangeException(
                $"Vertex Buffer items length cannot greater than '{MaxVertexBuffers}'.");
    }
#endif

    private void SetRenderTarget(ITextureView rt, ITextureView? depthStencil)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateGpuObject(rt);
        if (depthStencil is not null)
            ValidateGpuObject(depthStencil);
#endif

        NativeApis.js_write_i32(pTmpMem, rt.Handle.ToInt32());
        js_rengine_cmdbuffer_setrts(
            Handle, pTmpMem, 1,
            depthStencil?.Handle ?? IntPtr.Zero,
            0x0);
    }

    private void SetRenderTargets(ITextureView[] rts, ITextureView? depthStencil)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
        for (var i = 0; i < rts.Length; ++i)
        {
#if RENGINE_VALIDATIONS
            ValidateGpuObject(rts[i]);
#endif
            NativeApis.js_write_i32(pTmpMem + i, rts[0].Handle.ToInt32());
        }

        js_rengine_cmdbuffer_setrts(
            Handle, pTmpMem, rts.Length,
            depthStencil?.Handle ?? IntPtr.Zero,
            0x0);
    }

    public ICommandBuffer SetRT(ITextureView rt, ITextureView? depthStencil)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateGpuObject(rt);
        if (depthStencil is not null)
            ValidateGpuObject(depthStencil);
#endif
        SetRenderTarget(rt, depthStencil);
        return this;
    }

    public ICommandBuffer SetRTs(ITextureView[] rts, ITextureView? depthStencil)
    {
        SetRenderTargets(rts, depthStencil);
        return this;
    }

    private float[] pTmpColor = new float[4];
    public unsafe ICommandBuffer ClearRT(ITextureView renderTarget, in Color clearColor)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateGpuObject(renderTarget);
#endif
        pTmpColor[0] = clearColor.R / 255.0f;
        pTmpColor[1] = clearColor.G / 255.0f;
        pTmpColor[2] = clearColor.B / 255.0f;
        pTmpColor[3] = clearColor.A / 255.0f;

        fixed (void* colorPtr = pTmpColor)
            NativeApis.js_memcpy(colorPtr, pTmpMem, pTmpColor.Length * sizeof(float));

        js_rengine_cmdbuffer_clearrt(
            Handle,
            renderTarget.Handle,
            pTmpMem,
            0x0);
        return this;
    }

    public ICommandBuffer ClearDepth(ITextureView depthStencil, ClearDepthStencil clearFlags, float depth, byte stencil)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateGpuObject(depthStencil);
#endif
        
        js_rengine_cmdbuffer_cleardepth(
            Handle,
            depthStencil.Handle,
            (int)clearFlags,
            depth,
            stencil,
            0x0);
        return this;
    }

    public ICommandBuffer SetPipeline(IPipelineState pipelineState)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateGpuObject(pipelineState);
#endif
        js_rengine_cmdbuffer_setpipeline(Handle, pipelineState.Handle);
        return this;
    }

    public ICommandBuffer SetPipeline(IComputePipelineState pipelineState)
    {
        throw new NotSupportedException();
    }

    public ICommandBuffer SetVertexBuffer(IBuffer buffer, bool reset = true)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateGpuObject(buffer);
#endif

        NativeApis.js_write_i32(pTmpMem, buffer.Handle.ToInt32());
        NativeApis.js_write_float(pTmpMem + 1, 0.0f);
        js_rengine_cmdbuffer_setvbuffer(
            Handle, 0, 1, 
            pTmpMem, pTmpMem + 1, reset ? 0x1 : 0x0,
            0x0);
        return this;
    }

    public ICommandBuffer SetVertexBuffers(uint startSlot, IBuffer[] buffers, bool reset = true)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateVertexBuffer(buffers);
#endif

        for (var i = 0; i < buffers.Length; ++i)
        {
#if RENGINE_VALIDATIONS
            ValidateGpuObject(buffers[i]);
#endif
            NativeApis.js_write_i32(pTmpMem + i, buffers[i].Handle.ToInt32());
            NativeApis.js_write_i32(pTmpMem + buffers.Length + i, 0);
        }
        
        js_rengine_cmdbuffer_setvbuffer(
            Handle, (int)startSlot, buffers.Length,
            pTmpMem, pTmpMem + buffers.Length, reset ? 0x1 : 0x0,
            0x0);
        return this;
    }

    public ICommandBuffer SetVertexBuffers(uint startSlot, IBuffer[] buffers, ulong[] offsets, bool reset = true)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (offsets.Length != buffers.Length)
            throw new ArgumentException("Offsets Length must be the same size of Vertex Buffer Length.");
        ValidateVertexBuffer(buffers);
#endif

        for (var i = 0; i < buffers.Length; ++i)
        {
#if RENGINE_VALIDATIONS
            ValidateGpuObject(buffers[i]);
#endif
            NativeApis.js_write_i32(pTmpMem + i, buffers[i].Handle.ToInt32());
            NativeApis.js_write_i32(pTmpMem + buffers.Length + i, (int)offsets[i]);
        }
        
        js_rengine_cmdbuffer_setvbuffer(
            Handle, (int)startSlot, buffers.Length, pTmpMem, pTmpMem + buffers.Length,
            reset ? 0x1 : 0x0, 0x0);
        return this;
    }

    public ICommandBuffer SetIndexBuffer(IBuffer buffer, ulong byteOffset = 0)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateGpuObject(buffer);
#endif
        NativeApis.js_write_i32(pTmpMem, buffer.Handle.ToInt32());
        js_rengine_cmdbuffer_setibuffer(
            Handle, pTmpMem, (int)byteOffset, 0x0);
        return this;
    }

    public ICommandBuffer CommitBindings(IShaderResourceBinding resourceBinding)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateGpuObject(resourceBinding);
#endif
        
        js_rengine_cmdbuffer_commitbindings(
            Handle, resourceBinding.Handle, 0x0);
        return this;
    }

    public unsafe ICommandBuffer Draw(DrawArgs args)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
        var drawArgs = new DrawArgsDto(args);
        fixed (void* argsPtr = drawArgs)
            NativeApis.js_memcpy(pTmpMem, argsPtr, Unsafe.SizeOf<DrawArgsDto>());
        js_rengine_cmdbuffer_draw(handle, pTmpMem);
        return this;
    }

    public unsafe ICommandBuffer Draw(DrawIndexedArgs args)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
        var drawArgs = new DrawIndexedArgsDto(args);
        fixed(void* argsPtr = drawArgs)
            NativeApis.js_memcpy(pTmpMem, argsPtr, Unsafe.SizeOf<DrawIndexedArgsDto>());
        js_rengine_cmdbuffer_drawindexed(Handle, pTmpMem);
        return this;
    }

    public ICommandBuffer Compute(ComputeArgs args)
    {
        throw new NotSupportedException();
    }

    public ICommandBuffer SetBlendFactors(in Color color)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
        js_rengine_cmdbuffer_setblendfactors(
            Handle,
            color.R / 255.0f,
            color.G / 255.0f,
            color.B / 255.0f,
            color.A / 255.0f);
        return this;
    }

    public unsafe ICommandBuffer SetViewports(Viewport[] viewports, uint rtWidth, uint rtHeight)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
        var offset = 0;
        var sizeOf = Unsafe.SizeOf<ViewportDto>();
        
        for (var i = 0; i < viewports.Length; ++i)
        {
            var viewport = new ViewportDto(viewports[i]);
            fixed(void* ptr = viewport)
                NativeApis.js_memcpy(ptr, pTmpMem + offset, sizeOf);
            offset += sizeOf;
        }

        js_rengine_cmdbuffer_setviewports(
            Handle,
            pTmpMem,
            viewports.Length,
            (int)rtWidth,
            (int)rtHeight,
            0x0);
        return this;
    }

    public unsafe ICommandBuffer SetViewport(Viewport viewport, uint rtWidth, uint rtHeight)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
        var sizeOf = Unsafe.SizeOf<ViewportDto>();
        var view = new ViewportDto(viewport);
        
        fixed(void* viewPtr = view)
            NativeApis.js_memcpy(viewPtr, pTmpMem, sizeOf);
        
        js_rengine_cmdbuffer_setviewports(
            Handle,
            pTmpMem,
            1,
            (int)rtWidth, (int)rtHeight, 0x0);
        return this;
    }

    public unsafe ICommandBuffer SetScissors(IntRect[] scissors, uint rtWidth, uint rtHeight)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
        fixed(void* ptr = scissors.AsSpan())
            NativeApis.js_memcpy(ptr, pTmpMem, Unsafe.SizeOf<IntRect>() * scissors.Length);
       
        js_rengine_cmdbuffer_setscissors(
            Handle,
            pTmpMem,
            scissors.Length,
            (int)rtWidth,
            (int)rtHeight);
        return this;
    }

    public unsafe ICommandBuffer SetScissor(IntRect scissor, uint rtWidth, uint rtHeight)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
        
        fixed(void* ptr = scissor)
            NativeApis.js_memcpy(ptr, pTmpMem, Unsafe.SizeOf<IntRect>());
        
        js_rengine_cmdbuffer_setscissors(
            Handle,
            pTmpMem,
            1,
            (int)rtWidth,
            (int)rtHeight);
        return this;
    }
    
    public unsafe Span<T> Map<T>(IBuffer buffer, MapType mapType, MapFlags mapFlags) where T : unmanaged
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateGpuObject(buffer);
#endif
        if(pMappedData.Length > 0)
            pPool.Return(pMappedData);
        pPinnedHandle?.Free();

        pMappedData = pPool.Rent((int)buffer.Size);
        pPinnedHandle = GCHandle.Alloc(pMappedData);

        var len = (int)buffer.Size / Unsafe.SizeOf<T>();
        return new Span<T>(pPinnedHandle.Value.AddrOfPinnedObject().ToPointer(), len);
    }

    public unsafe IntPtr Map(IBuffer buffer, MapType mapType, MapFlags mapFlags)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateGpuObject(buffer);
#endif
        
        if(pMappedData.Length > 0)
            pPool.Return(pMappedData);
        if(pPinnedHandle.HasValue)
            pPinnedHandle.Value.Free();

        pMappedData = pPool.Rent((int)buffer.Size);
        pPinnedHandle = GCHandle.Alloc(pMappedData);
        pMappedPtr = js_rengine_cmdbuffer_map(
            Handle, buffer.Handle, (int)mapType, (int)mapFlags);
        
        var ptr = pPinnedHandle.Value.AddrOfPinnedObject();
        // Copy Buffer Data into Pinned Reference
        if (mapType is MapType.Read or MapType.ReadWrite)
        {
        }
        return pPinnedHandle.Value.AddrOfPinnedObject();
    }

    public unsafe ICommandBuffer Unmap(IBuffer buffer, MapType mapType)
    {
#if RENGINE_VALIDATIONS
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ValidateGpuObject(buffer);
#endif
        if (pPinnedHandle is null || pMappedData.Length == 0)
            return this;

        // fixed (void* ptr = pMappedData.AsSpan())
        // {
        //     js_rengine_cmdbuffer_map(
        //         Handle,
        //         buffer.Handle,
        //         (in))
        // }
        
        
        pPinnedHandle?.Free();
        pPool.Return(pMappedData);
        pMappedData = [];
        return this;
    }

    public ICommandBuffer Copy(CopyTextureInfo copyInfo)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, T data) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, byte[] data)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, Span<T> data) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, ReadOnlySpan<T> data) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, ulong size, IntPtr data)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer BeginDebugGroup(string name, Color color)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer EndDebugGroup()
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer InsertDebugLabel(string label, Color color)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer Begin(uint immediateContextId)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer FinishFrame()
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer FinishCommandList(out ICommandList commandList)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer TransitionShaderResource(IPipelineState pipelineState, IShaderResourceBinding binding)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer SetStencilRef(uint stencilRef)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer InvalidateState()
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer NextSubpass()
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer GenerateMips(ITextureView textureView)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer TransitionResourceStates(StateTransitionDesc[] resourceBarriers)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer ResolveTextureSubresource(ITexture srcTexture, ITexture dstTexture,
        ResolveTextureSubresourceDesc resolveDesc)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer ExecuteCommandList(ICommandList[] list)
    {
        throw new NotImplementedException();
    }
}