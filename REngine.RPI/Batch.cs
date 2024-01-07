using System.Numerics;
using REngine.RHI;
using ValueType = REngine.RHI.ValueType;

namespace REngine.RPI;

public struct BatchRenderInfo
{
    public ITextureView DefaultRenderTarget;
    public ITextureView DefaultDepthStencil;
    public ICommandBuffer CommandBuffer;
}
public abstract class Batch : IComparable<Batch>, IDisposable
{
    public int Id { get; internal set; }
    public bool IsDisposed { get; private set; }
    public IPipelineState? PipelineState { get; set; }
    public IShaderResourceBinding? ShaderResourceBinding { get; set; }
    public abstract void Render(BatchRenderInfo batchRenderInfo);
    public virtual int CompareTo(Batch? other)
    {
        return GetSortIndex() - (other?.GetSortIndex() ?? 0);
    }

    public virtual int GetSortIndex() => 0;

    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        OnDispose();
        Id = -1;
    }

    protected virtual void OnDispose(){}
}

public class QuadBatch : Batch
{
    public uint NumInstances { get; set; } = 1;
    
    public override void Render(BatchRenderInfo batchRenderInfo)
    {
        if (PipelineState is null)
            return;
        batchRenderInfo.CommandBuffer.SetPipeline(PipelineState);
        if (ShaderResourceBinding is not null)
            batchRenderInfo.CommandBuffer.CommitBindings(ShaderResourceBinding);
        batchRenderInfo.CommandBuffer.Draw(new DrawArgs
        {
            NumVertices = 4,
            NumInstances = NumInstances,
        });
    }
}

public class IndexedBatch : Batch
{
    public uint StartSlot { get; set; }
    public uint BaseVertex { get; set; }
    public uint NumIndices { get; set; }
    public uint FirstIndexLocation { get; set; }
    public ValueType IndexType { get; set; } = ValueType.Int32;
    public IBuffer[] VertexBuffers { get; set; } = [];
    public ulong[] Offsets { get; set; } = [];
    public IBuffer? IndexBuffer { get; set; }

    public override void Render(BatchRenderInfo batchRenderInfo)
    {
        if (PipelineState is null || VertexBuffers.Length == 0 || IndexBuffer is null)
            return;
        if (Offsets.Length != VertexBuffers.Length)
            throw new Exception($"{nameof(Offsets.Length)} must be the same size of {nameof(VertexBuffers.Length)}");
        batchRenderInfo.CommandBuffer
            .SetPipeline(PipelineState)
            .SetVertexBuffers(StartSlot, VertexBuffers, Offsets, false);

        if (ShaderResourceBinding is not null)
            batchRenderInfo.CommandBuffer.CommitBindings(ShaderResourceBinding);
        batchRenderInfo.CommandBuffer.Draw(new DrawIndexedArgs()
        {
            NumInstances = 1,
            BaseVertex = BaseVertex,
            IndexType = IndexType,
            NumIndices = NumIndices,
            FirstIndexLocation = FirstIndexLocation
        });
    }
}