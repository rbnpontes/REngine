using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Sequential, Pack =  4)]
internal unsafe struct DrawArgsDto
{
    public uint NumVertices;
    public uint NumInstances;
    public uint StartVertexLocation;
    public uint FirstInstanceLocation;

    public DrawArgsDto()
    {
        NumInstances = 1;
        NumVertices = StartVertexLocation = FirstInstanceLocation = 0;
    }

    public DrawArgsDto(DrawArgs args)
    {
        NumInstances = args.NumInstances;
        NumVertices = args.NumVertices;
        StartVertexLocation = args.StartVertexLocation;
        FirstInstanceLocation = args.FirstInstanceLocation;
    }

    public ref DrawArgsDto GetPinnableReference()
    {
        return ref this;
    }
}

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct DrawIndexedArgsDto
{
    [FieldOffset(0)]
    public uint NumIndices;
    [FieldOffset(4)]
    public ValueType IndexType;
    [FieldOffset(8)]
    public uint NumInstances;
    [FieldOffset(12)]
    public uint FirstIndexLocation;
    [FieldOffset(16)]
    public uint BaseVertex;
    [FieldOffset(20)]
    public uint FirstInstanceLocation;

    public DrawIndexedArgsDto()
    {
        this = default(DrawIndexedArgsDto);
        IndexType = ValueType.UInt32;
        NumInstances = 1;
    }

    public DrawIndexedArgsDto(DrawIndexedArgs args)
    {
        NumIndices = args.NumIndices;
        IndexType = args.IndexType;
        NumInstances = args.NumInstances;
        FirstIndexLocation = args.FirstIndexLocation;
        BaseVertex = args.BaseVertex;
        FirstInstanceLocation = args.FirstInstanceLocation;
    }

    public ref DrawIndexedArgsDto GetPinnableReference()
    {
        return ref this;
    }
}