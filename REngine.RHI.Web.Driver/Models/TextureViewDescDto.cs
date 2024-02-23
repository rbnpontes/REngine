using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct TextureViewDescDto
{
    [FieldOffset(0)]
    public byte ViewType;
    [FieldOffset(4)]
    public byte Dimension;
    [FieldOffset(8)]
    public ushort Format;
    [FieldOffset(12)]
    public uint MostDetailedMip;
    [FieldOffset(16)]
    public uint MipLevels;
    [FieldOffset(20)]
    public uint FirstSlice;
    [FieldOffset(24)]
    public uint SlicesCount;
    [FieldOffset(28)]
    public uint AccessFlags;
    [FieldOffset(32)]
    public byte AllowMipMapGeneration;
    
    public TextureViewDescDto() {}

    public TextureViewDescDto(TextureViewDesc desc)
    {
        ViewType = (byte)desc.ViewType;
        Dimension = (byte)desc.Dimension;
        Format = (ushort)desc.Format;
        MostDetailedMip = desc.MostDetailedMip;
        MipLevels = desc.MipLevels;
        FirstSlice = desc.FirstSlice;
        SlicesCount = desc.SlicesCount;
        AccessFlags = (uint)desc.AccessFlags;
        AllowMipMapGeneration = (byte)(desc.AllowMipMapGeneration ? 0x1 : 0x0);
    }

    public void CopyTo(ref TextureViewDesc desc)
    {
        desc.ViewType = (TextureViewType)ViewType;
        desc.Dimension = (TextureDimension)Dimension;
        desc.Format = (TextureFormat)Format;
        desc.MostDetailedMip = desc.MostDetailedMip;
        desc.MipLevels = desc.MipLevels;
        desc.FirstSlice = desc.FirstSlice;
        desc.SlicesCount = desc.SlicesCount;
        desc.AccessFlags = (UavAccessFlags)AccessFlags;
        desc.AllowMipMapGeneration = AllowMipMapGeneration == 0x1;
    }

    public ref TextureViewDescDto GetPinnableReference()
    {
        return ref this;
    }
}