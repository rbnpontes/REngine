using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal unsafe struct InputLayoutElementDescDto
{
    public uint InputIndex;
    public uint BufferIndex;
    public uint BufferStride;
    public uint ElementOffset;
    public uint InstanceStepRate;
    public byte ElementType;
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