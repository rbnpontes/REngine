using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal unsafe struct ShaderCreateInfoDto
{
    public IntPtr Name;
    public uint Type;
    public IntPtr SourceCode;
    public IntPtr ByteCode;
    public int ByteCodeLength;
    public IntPtr MacroKeysPtr;
    public IntPtr MacroValuesPtr;
    public int MacroEntriesCount;

    public ShaderCreateInfoDto()
    {
        this = default;
        Name = SourceCode = ByteCode = MacroKeysPtr = MacroValuesPtr = IntPtr.Zero;
    }

    public ShaderCreateInfoDto(ShaderCreateInfo shaderCi)
    {
        Type = (uint)shaderCi.Type;
        ByteCodeLength = shaderCi.ByteCode.Length;
        MacroEntriesCount = shaderCi.Macros.Count;
        Name = SourceCode = ByteCode = MacroKeysPtr = MacroValuesPtr = IntPtr.Zero;
    }

    public ref ShaderCreateInfoDto GetPinnableReference()
    {
        return ref this;
    }
}