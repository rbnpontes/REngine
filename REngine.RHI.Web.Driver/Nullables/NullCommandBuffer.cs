using System.Drawing;
using REngine.Core.Mathematics;

namespace REngine.RHI.Web.Driver;

internal class NullCommandBuffer : ICommandBuffer
{
    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed => false;
    public event EventHandler? OnDispose;
    
    public void Dispose()
    {
    }
    public ICommandBuffer SetRT(ITextureView rt, ITextureView? depthStencil)
    {
        return this;
    }

    public ICommandBuffer SetRTs(ITextureView[] rts, ITextureView? depthStencil)
    {
        return this;
    }

    public ICommandBuffer ClearRT(ITextureView renderTarget, in Color clearColor)
    {
        return this;
    }

    public ICommandBuffer ClearDepth(ITextureView depthStencil, ClearDepthStencil clearFlags, float depth, byte stencil)
    {
        return this;
    }

    public ICommandBuffer SetPipeline(IPipelineState pipelineState)
    {
        return this;
    }

    public ICommandBuffer SetPipeline(IComputePipelineState pipelineState)
    {
        return this;
    }

    public ICommandBuffer SetVertexBuffer(IBuffer buffer, bool reset = true)
    {
        return this;
    }

    public ICommandBuffer SetVertexBuffers(uint startSlot, IBuffer[] buffers, bool reset = true)
    {
        return this;
    }

    public ICommandBuffer SetVertexBuffers(uint startSlot, IBuffer[] buffers, ulong[] offsets, bool reset = true)
    {
        return this;
    }

    public ICommandBuffer SetIndexBuffer(IBuffer buffer, ulong byteOffset = 0)
    {
        return this;
    }

    public ICommandBuffer CommitBindings(IShaderResourceBinding resourceBinding)
    {
        return this;
    }

    public ICommandBuffer Draw(DrawArgs args)
    {
        return this;
    }

    public ICommandBuffer Draw(DrawIndexedArgs args)
    {
        return this;
    }

    public ICommandBuffer Compute(ComputeArgs args)
    {
        return this;
    }

    public ICommandBuffer SetBlendFactors(in Color color)
    {
        return this;
    }

    public ICommandBuffer SetViewports(Viewport[] viewports, uint rtWidth, uint rtHeight)
    {
        return this;
    }

    public ICommandBuffer SetViewport(Viewport viewports, uint rtWidth, uint rtHeight)
    {
        return this;
    }

    public ICommandBuffer SetScissors(IntRect[] scissors, uint rtWidth, uint rtHeight)
    {
        return this;
    }

    public ICommandBuffer SetScissor(IntRect scissor, uint rtWidth, uint rtHeight)
    {
        return this;
    }

    public Span<T> Map<T>(IBuffer buffer, MapType mapType, MapFlags mapFlags) where T : unmanaged
    {
        T[] arr = [];
        return arr.AsSpan();
    }

    public IntPtr Map(IBuffer buffer, MapType mapType, MapFlags mapFlags)
    {
        return IntPtr.Zero;
    }

    public ICommandBuffer Unmap(IBuffer buffer, MapType mapType)
    {
        return this;
    }

    public ICommandBuffer Copy(CopyTextureInfo copyInfo)
    {
        return this;
    }

    public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, T data) where T : unmanaged
    {
        return this;
    }

    public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, byte[] data)
    {
        return this;
    }

    public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, Span<T> data) where T : unmanaged
    {
        return this;
    }

    public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, ReadOnlySpan<T> data) where T : unmanaged
    {
        return this;
    }

    public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, ulong size, IntPtr data)
    {
        return this;
    }

    public ICommandBuffer BeginDebugGroup(string name, Color color)
    {
        return this;
    }

    public ICommandBuffer EndDebugGroup()
    {
        return this;
    }

    public ICommandBuffer InsertDebugLabel(string label, Color color)
    {
        return this;
    }

    public ICommandBuffer Begin(uint immediateContextId)
    {
        return this;
    }

    public ICommandBuffer FinishFrame()
    {
        return this;
    }

    public ICommandBuffer FinishCommandList(out ICommandList commandList)
    {
        commandList = NullCommandList.Instance;
        return this;
    }

    public ICommandBuffer TransitionShaderResource(IPipelineState pipelineState, IShaderResourceBinding binding)
    {
        return this;
    }

    public ICommandBuffer SetStencilRef(uint stencilRef)
    {
        return this;
    }

    public ICommandBuffer InvalidateState()
    {
        return this;
    }

    public ICommandBuffer NextSubpass()
    {
        return this;
    }

    public ICommandBuffer GenerateMips(ITextureView textureView)
    {
        return this;
    }

    public ICommandBuffer TransitionResourceStates(StateTransitionDesc[] resourceBarriers)
    {
        return this;
    }

    public ICommandBuffer ResolveTextureSubresource(ITexture srcTexture, ITexture dstTexture,
        ResolveTextureSubresourceDesc resolveDesc)
    {
        return this;
    }

    public ICommandBuffer ExecuteCommandList(ICommandList[] list)
    {
        return this;
    }

    public static readonly ICommandBuffer Instance = new NullCommandBuffer();
}