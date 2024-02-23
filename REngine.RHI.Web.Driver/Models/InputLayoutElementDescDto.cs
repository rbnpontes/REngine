using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct InputLayoutElementDescDto
{
    [FieldOffset(0)]
    public uint InputIndex;
    [FieldOffset(4)]
    public uint BufferIndex;
    [FieldOffset(8)]
    public uint BufferStride;
    [FieldOffset(12)]
    public uint ElementOffset;
    [FieldOffset(16)]
    public uint InstanceStepRate;
    [FieldOffset(20)]
    public byte ElementType;
    [FieldOffset(24)]
    public bool Normalized;
    
    public InputLayoutElementDescDto(){}

    public InputLayoutElementDescDto(PipelineInputLayoutElementDesc desc)
    {
        InputIndex = desc.InputIndex;
        BufferIndex = desc.Input.BufferIndex;
        BufferStride = desc.Input.BufferStride;
        ElementOffset = desc.Input.ElementOffset;
        InstanceStepRate = desc.Input.InstanceStepRate;
        ElementType = (byte)desc.Input.ElementType;
        Normalized = desc.Input.IsNormalized;
    }

    public ref InputLayoutElementDescDto GetPinnableReference()
    {
        return ref this;
    }
}