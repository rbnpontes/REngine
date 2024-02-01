using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal unsafe struct SwapChainDescDto()
{
    public uint Width;
    public uint Height;
    public uint ColorFormat;
    public uint DepthFormat;
    public uint Usage;
    public uint Transform;
    public uint BufferCount;
    public float DefaultDepthValue;
    public uint DefaultStencilValue;
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
    public static void ReadSwapChainDesc(IntPtr ptr, ref SwapChainDescDto output)
    {
        fixed (void* dstPtr = output)
            NativeApis.js_memcpy(ptr, dstPtr, Unsafe.SizeOf<SwapChainDescDto>());
    }

    private ref SwapChainDescDto GetPinnableReference()
    {
        return ref this;
    }
}