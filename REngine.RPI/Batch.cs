using REngine.RHI;
using ValueType = REngine.RHI.ValueType;

namespace REngine.RPI;

public abstract class Batch
{
    public IPipelineState? PipelineState { get; set; }
    public IShaderResourceBinding? ShaderResourceBinding { get; set; }
    public abstract void Render(ICommandBuffer command);
}

public class QuadBatch : Batch
{
    public uint NumInstances { get; set; } = 1;
    
    public override void Render(ICommandBuffer command)
    {
        if (PipelineState is null)
            return;
        command.SetPipeline(PipelineState);
        if (ShaderResourceBinding is not null)
            command.CommitBindings(ShaderResourceBinding);
        command.Draw(new DrawArgs
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

    public override void Render(ICommandBuffer command)
    {
        if (PipelineState is null || VertexBuffers.Length == 0 || IndexBuffer is null)
            return;
        if (Offsets.Length != VertexBuffers.Length)
            throw new Exception($"{nameof(Offsets.Length)} must be the same size of {nameof(VertexBuffers.Length)}");
        command
            .SetPipeline(PipelineState)
            .SetVertexBuffers(StartSlot, VertexBuffers, Offsets, false);

        if (ShaderResourceBinding is not null)
            command.CommitBindings(ShaderResourceBinding);
        command.Draw(new DrawIndexedArgs()
        {
            NumInstances = 1,
            BaseVertex = BaseVertex,
            IndexType = IndexType,
            NumIndices = NumIndices,
            FirstIndexLocation = FirstIndexLocation
        });
    }
}