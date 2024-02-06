using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct SwapChainDescDto()
{
    [FieldOffset(0)]
    public uint Width;
    [FieldOffset(4)]
    public uint Height;
    [FieldOffset(8)]
    public uint ColorFormat;
    [FieldOffset(12)]
    public uint DepthFormat;
    [FieldOffset(16)]
    public uint Usage;
    [FieldOffset(20)]
    public uint Transform;
    [FieldOffset(24)]
    public uint BufferCount;
    [FieldOffset(28)]
    public float DefaultDepthValue;
    [FieldOffset(32)]
    public uint DefaultStencilValue;
    [FieldOffset(36)]
    public bool IsPrimary;

    public SwapChainDescDto(SwapChainDesc desc) : this()
    {
        Width = desc.Size.Width;
        Height = desc.Size.Height;
        ColorFormat = (uint)desc.Formats.Color;
        DepthFormat = (uint)desc.Formats.Depth;
        Usage = (uint)desc.Usage;
        Transform = (uint)desc.Transform;
        BufferCount = desc.BufferCount;
        DefaultDepthValue = (uint)desc.DefaultDepthValue;
        DefaultStencilValue = desc.DefaultStencilValue;
        IsPrimary = desc.IsPrimary;
    }

    public static IntPtr CreateSwapChainPtr(ref SwapChainDescDto src)
    {
        var size = Unsafe.SizeOf<SwapChainDescDto>();
        var ptr = NativeApis.js_malloc(size);
        fixed(void* srcPtr = src)
            NativeApis.js_memcpy(srcPtr, ptr, size);
        return ptr;
    }

    public ref SwapChainDescDto GetPinnableReference()
    {
        return ref this;
    }
}