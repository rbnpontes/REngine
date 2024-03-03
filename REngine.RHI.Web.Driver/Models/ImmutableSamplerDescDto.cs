using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct ImmutableSamplerDescDto
{
    [FieldOffset(0)]
    public IntPtr Name;
    [FieldOffset(4)]
    public byte Sampler_FilterMode;
    [FieldOffset(8)]
    public byte Sampler_Anisotropy;
    [FieldOffset(12)]
    public bool Sampler_ShadowCmp;
    [FieldOffset(16)]
    public byte Sampler_AddressMode_U;
    [FieldOffset(20)]
    public byte Sampler_AddressMode_V;
    [FieldOffset(24)]
    public byte Sampler_AddressMode_W;

    public ImmutableSamplerDescDto()
    {
        this = default;
        Name = IntPtr.Zero;
    }

    public ImmutableSamplerDescDto(ImmutableSamplerDesc samplerDesc)
    {
        Name = IntPtr.Zero;
        Sampler_FilterMode = (byte)samplerDesc.Sampler.FilterMode;
        Sampler_Anisotropy = samplerDesc.Sampler.Anisotropy;
        Sampler_ShadowCmp = samplerDesc.Sampler.ShadowCompare;
        Sampler_AddressMode_U = (byte)samplerDesc.Sampler.AddressModes.U;
        Sampler_AddressMode_V = (byte)samplerDesc.Sampler.AddressModes.V;
        Sampler_AddressMode_W = (byte)samplerDesc.Sampler.AddressModes.W;
    }

    public ref ImmutableSamplerDescDto GetPinnableReference()
    {
        return ref this;
    }
}