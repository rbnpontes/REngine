using System.Runtime.InteropServices;

namespace REngine.RHI.NativeDriver.NativeStructs;

internal struct BufferViewCreateDescDTO
{
    public IntPtr name;
    public byte viewType;
    public byte format_valueType;
    public byte format_numComponents;
    public byte format_isNormalized;
    public ulong byteOffset;
    public ulong byteWidth;

    public static void Fill(in BufferViewDesc desc, out BufferViewCreateDescDTO output)
    {
        output.name = string.IsNullOrEmpty(desc.Name) ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(string.Intern(desc.Name));
        output.viewType = (byte)desc.ViewType;
        output.format_valueType = (byte)desc.Format.ValueType;
        output.format_numComponents = desc.Format.NumComponents;
        output.format_isNormalized = (byte)(desc.Format.IsNormalized ? 1 : 0);
        output.byteOffset = desc.ByteOffset;
        output.byteWidth = desc.ByteWidth;
    }

    public static void Fill(in BufferViewCreateDescDTO desc, out BufferViewDesc output)
    {
        output.Name = desc.name == IntPtr.Zero
            ? string.Empty
            : string.Intern(Marshal.PtrToStringAnsi(desc.name) ?? string.Empty);
        output.ViewType = (BufferViewType)desc.viewType;
        output.Format.ValueType = (ValueType)desc.format_valueType;
        output.Format.NumComponents = desc.format_numComponents;
        output.Format.IsNormalized = desc.format_isNormalized == 1;
        output.ByteOffset = desc.byteOffset;
        output.ByteWidth = desc.byteWidth;
    }
}