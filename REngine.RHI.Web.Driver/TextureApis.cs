using System.Runtime.InteropServices.JavaScript;

namespace REngine.RHI.Web.Driver;

internal partial class TextureImpl
{
    [JSImport("_rengine_texture_getdesc", Constants.LibName)]
    public static partial void js_rengine_texture_getdesc(IntPtr ptr, IntPtr descPtr);
    [JSImport("_rengine_texture_get_gpuhandle", Constants.LibName)]
    public static partial IntPtr js_rengine_texture_get_gpuhandle(IntPtr ptr);
    [JSImport("_rengine_texture_get_state", Constants.LibName)]
    public static partial int js_rengine_texture_get_state(IntPtr ptr);
    
    [JSImport("_rengine_texture_set_state", Constants.LibName)]
    public static partial void js_rengine_texture_set_state(IntPtr ptr, int value);
    
    [JSImport("_rengine_texture_getdefaultview", Constants.LibName)]
    public static partial void js_rengine_texture_getdefaultview(IntPtr ptr, int viewType, IntPtr resultPtr);
}