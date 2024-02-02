using System.Runtime.InteropServices.JavaScript;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace REngine.RHI.Web.Driver;

internal partial class NativeObject
{
    [JSImport("_rengine_object_set_release_callback", Constants.LibName)]
    public static partial void js_rengine_object_set_release_callback(IntPtr ptr, [JSMarshalAs<JSType.Function>] Action action);

    [JSImport("_rengine_object_releaseref", Constants.LibName)]
    public static partial void js_rengine_object_release(IntPtr ptr);

    [JSImport("_rengine_object_strongref_count", Constants.LibName)]
    public static partial int js_rengine_object_strongref_count(IntPtr ptr);

    [JSImport("_rengine_object_weakref_count", Constants.LibName)]
    public static partial int js_rengine_object_weakref_count(IntPtr ptr);

    [JSImport("_rengine_object_addref", Constants.LibName)]
    public static partial void js_rengine_object_addref(IntPtr ptr);

    [JSImport("_rengine_object_getname", Constants.LibName)]
    public static partial string js_rengine_object_getname(IntPtr handle);
}