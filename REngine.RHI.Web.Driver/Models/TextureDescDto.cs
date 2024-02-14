using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct TextureDescDto
{
    [FieldOffset(0)]
    public IntPtr Name;
    [FieldOffset(4)]
    public byte Dimension;
    [FieldOffset(8)]
    public uint Width;
    [FieldOffset(12)]
    public uint Height;
    [FieldOffset(16)]
    public uint ArraySizeOrDepth;
    [FieldOffset(20)]
    public ushort Format;
    [FieldOffset(24)]
    public uint MipLevels;
    [FieldOffset(28)]
    public uint SampleCount;
    [FieldOffset(32)]
    public uint BindFlags;
    [FieldOffset(36)]
    public byte Usage;
    [FieldOffset(40)]
    public byte AccessFlags;
    [FieldOffset(44)]
    public byte TextureFlags;

    [FieldOffset(48)]
    public ushort Clear_Format;
    [FieldOffset(52)]
    public float Clear_R;
    [FieldOffset(56)]
    public float Clear_G;
    [FieldOffset(60)]
    public float Clear_B;
    [FieldOffset(64)]
    public float Clear_A;

    [FieldOffset(68)]
    public float Clear_Depth;
    [FieldOffset(72)]
    public byte Clear_Stencil;
    
    public TextureDescDto() {}

    public TextureDescDto(TextureDesc desc)
    {
        Name = string.IsNullOrEmpty(desc.Name) ? IntPtr.Zero : NativeApis.js_alloc_string(desc.Name);
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