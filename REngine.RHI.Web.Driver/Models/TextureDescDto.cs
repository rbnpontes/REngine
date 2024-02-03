using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal unsafe struct TextureDescDto
{
    public IntPtr Name;
    public byte Dimension;
    public uint Width;
    public uint Height;
    public uint ArraySizeOrDepth;
    public ushort Format;
    public uint MipLevels;
    public uint SampleCount;
    public uint BindFlags;
    public byte Usage;
    public byte AccessFlags;
    public byte TextureFlags;

    public ushort Clear_Format;
    public float Clear_R;
    public float Clear_G;
    public float Clear_B;
    public float Clear_A;

    public float Clear_Depth;
    public byte Clear_Stencil;
    
    public TextureDescDto() {}

    public TextureDescDto(TextureDesc desc, IntPtr namePtr)
    {
        Name = namePtr;
        Dimension = (byte)desc.Dimension;
        Width = desc.Size.Width;
        Height = desc.Size.Height;
        ArraySizeOrDepth = desc.ArraySizeOrDepth;
        Format = (ushort)desc.Format;
        MipLevels = desc.MipLevels;
        SampleCount = desc.SampleCount;
        BindFlags = (uint)desc.BindFlags;
        Usage = (byte)desc.Usage;
        AccessFlags = (byte)desc.AccessFlags;
        TextureFlags = (byte)desc.Flags;

        Clear_Format = (ushort)desc.ClearValue.Format;
        Clear_R = desc.ClearValue.R;
        Clear_G = desc.ClearValue.G;
        Clear_B = desc.ClearValue.B;
        Clear_A = desc.ClearValue.A;

        Clear_Depth = desc.ClearValue.Depth;
        Clear_Stencil = desc.ClearValue.Stencil;
    }

    public void CopyTo(ref TextureDesc desc)
    {
        desc.Name = Name == IntPtr.Zero ? string.Empty : string.Intern(NativeApis.js_get_string(Name));
        desc.Dimension = (TextureDimension)Dimension;
        desc.Size.Width = Width;
        desc.Size.Height = Height;
        desc.ArraySizeOrDepth = ArraySizeOrDepth;
        desc.Format = (TextureFormat)Format;
        desc.MipLevels = MipLevels;
        desc.SampleCount = SampleCount;
        desc.BindFlags = (BindFlags)BindFlags;
        desc.Usage = (Usage)Usage;
        desc.AccessFlags = (CpuAccessFlags)AccessFlags;
        desc.Flags = (TextureFlags)TextureFlags;

        desc.ClearValue.Format = (TextureFormat)Clear_Format;
        desc.ClearValue.R = Clear_R;
        desc.ClearValue.G = Clear_G;
        desc.ClearValue.B = Clear_B;
        desc.ClearValue.A = Clear_A;
        desc.ClearValue.Depth = Clear_Depth;
        desc.ClearValue.Stencil = Clear_Stencil;
    }

    public ref TextureDescDto GetPinnableReference()
    {
        return ref this;
    }
}