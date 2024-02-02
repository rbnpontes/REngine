using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using REngine.RHI.Web.Driver.Models;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace REngine.RHI.Web.Driver;

internal partial class SwapChainImpl
{
    [JSImport("_rengine_swapchain_get_desc", Constants.LibName)]
    private static partial void js_rengine_swapchain_get_desc(IntPtr swapChainPtr, IntPtr descPtr);

    [JSImport("_rengine_swapchain_resize", Constants.LibName)]
    private static partial void js_rengine_swapchain_resize(IntPtr ptr, int width, int height, int transform);
    public static unsafe SwapChainDesc GetDesc(IntPtr handle)
    {
        var descDto = new SwapChainDescDto();
        var descSize = Unsafe.SizeOf<SwapChainDescDto>();
        var descPtr = NativeApis.js_malloc(descSize);
        
        js_rengine_swapchain_get_desc(handle, descPtr);
        fixed (void* descDataPtr = descDto)
            NativeApis.js_memcpy(descPtr, descDataPtr, descSize);
        NativeApis.js_free(descPtr);
        
        return new SwapChainDesc()
        {
            Size = new SwapChainSize(descDto.Width, descDto.Height),
            Formats = new SwapChainFormats((TextureFormat)descDto.ColorFormat, (TextureFormat)descDto.DepthFormat),
            Usage = (SwapChainUsage)descDto.Usage,
            Transform = (SwapChainTransform)descDto.Transform,
            BufferCount = descDto.BufferCount,
            DefaultDepthValue = descDto.DefaultDepthValue,
            DefaultStencilValue = (byte)descDto.DefaultStencilValue,
            IsPrimary =  descDto.IsPrimary
        };
    }

    [JSImport("_rengine_swapchain_get_depthbuffer", Constants.LibName)]
    public static partial IntPtr js_rengine_swapchain_get_depthbuffer(IntPtr handle);

    [JSImport("_rengine_swapchain_get_backbuffer", Constants.LibName)]
    public static partial IntPtr js_rengine_swapchain_get_backbuffer(IntPtr handle);
}