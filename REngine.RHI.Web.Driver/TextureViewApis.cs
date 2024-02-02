using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using REngine.RHI.Web.Driver.Models;

namespace REngine.RHI.Web.Driver;

internal partial class TextureViewImpl
{
    [JSImport("_rengine_textureview_getparent", Constants.LibName)]
    private static partial void js_rengine_textureview_getparent(IntPtr ptr, IntPtr resultPtr);

    [JSImport("_rengine_textureview_getdesc", Constants.LibName)]
    public static partial void js_rengine_textureview_getdesc(IntPtr ptr, IntPtr descPtr);

    public static unsafe IntPtr GetTextureParentPtr(IntPtr ptr)
    {
        var result = new ResultNative();
        var sizeOf = Unsafe.SizeOf<ResultNative>();
        var resultPtr = NativeApis.js_malloc(sizeOf);
        
        js_rengine_textureview_getparent(ptr, resultPtr);
        fixed(void* dataPtr = result)
            NativeApis.js_memcpy(resultPtr, dataPtr, sizeOf);
        NativeApis.js_free(resultPtr);

        if (result.Error != IntPtr.Zero)
            throw new Exception(NativeApis.js_get_string(result.Error) ??
                                "Error has occurred while is trying to get parent texture.");
        return result.Value;
    }
}