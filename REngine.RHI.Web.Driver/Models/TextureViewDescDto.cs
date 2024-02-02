using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal unsafe struct TextureViewDescDto
{
    public byte ViewType;
    public byte Dimension;
    public ushort Format;
    public uint MostDetailedMip;
    public uint MipLevels;
    public uint FirstSlice;
    public uint SlicesCount;
    public uint AccessFlags;
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