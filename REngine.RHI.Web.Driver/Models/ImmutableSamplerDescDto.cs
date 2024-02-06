using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal unsafe struct ImmutableSamplerDescDto
{
    public IntPtr Name;
    public byte Sampler_FilterMode;
    public byte Sampler_Anisotropy;
    public bool Sampler_ShadowCmp;
    public byte Sampler_AddressMode_U;
    public byte Sampler_AddressMode_V;
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