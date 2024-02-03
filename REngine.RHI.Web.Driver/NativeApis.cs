using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
// ReSharper disable StringLiteralTypo

namespace REngine.RHI.Web.Driver;

internal partial class NativeApis
{
    [JSImport("malloc", Constants.LibName)]
    public static partial IntPtr js_malloc(int size);

    [JSImport("free", Constants.LibName)]
    public static partial void js_free(IntPtr ptrAddr);

    [JSImport("readI32", Constants.LibName)]
    public static partial int js_readi32(IntPtr addr);
    
    [JSImport("readF32", Constants.LibName)]
    public static partial double js_readf32(IntPtr addr);

    [JSImport("writeI32", Constants.LibName)]
    public static partial void js_writei32(IntPtr addr, int value);

    [JSImport("writeF32", Constants.LibName)]
    public static partial void js_writef32(IntPtr addr, double value);

    [JSImport("getPtrSize", Constants.LibName)]
    public static partial int js_get_ptr_size();

    [JSImport("memcpy", Constants.LibName)]
    private static partial void js_memcpy_v0([JSMarshalAs<JSType.MemoryView>]Span<byte> src, IntPtr dst, int @sizeof);

    [JSImport("memcpy", Constants.LibName)]
    private static partial void js_memcpy_v1(IntPtr src, [JSMarshalAs<JSType.MemoryView>] Span<byte> dst, int @sizeof);

    [JSImport("memcpy", Constants.LibName)]
    private static partial void js_memcpy_v2(IntPtr src, IntPtr dst, int @sizeof);

    [JSImport("memset", Constants.LibName)]
    public static partial void js_memset(IntPtr src, int value, int @sizeof);

    [JSImport("getLastMethodArgs", Constants.LibName)]
    public static partial int[] js_get_last_method_v0();
    
    [JSImport("registerFunction", Constants.LibName)]
    public static partial IntPtr js_register_function([JSMarshalAs<JSType.Function>] Action callback, string signature);

    [JSImport("unregisterFunction", Constants.LibName)]
    public static partial void js_unregister_function(IntPtr functPtr);
    [JSImport("getString", Constants.LibName)]
    public static partial string js_get_string(IntPtr ptr);

    [JSImport("allocString", Constants.LibName)]
    public static partial IntPtr js_alloc_string(string str);
    
    public static void js_memcpy(Span<byte> src, IntPtr dst, int length)
    {
        js_memcpy_v0(src, dst, length);
    }

    public static void js_memcpy(IntPtr src, Span<byte> dst, int length)
    {
        js_memcpy_v1(src, dst, length);
    }

    public static void js_memcpy(IntPtr src, IntPtr dst, int length)
    {
        js_memcpy_v2(src, dst, length);
    }

    public static unsafe void js_memcpy(void* src, IntPtr dst, int sizeOf)
    {
        var span = new Span<byte>(src, sizeOf);
        js_memcpy(span, dst, sizeOf);
    }

    public static unsafe void js_memcpy(IntPtr src, void* dst, int sizeOf)
    {
        var span = new Span<byte>(dst, sizeOf);
        js_memcpy(src, span, sizeOf);
    }

    [JSImport("querySelector", Constants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    public static partial object js_query_selector(string selector);

    [JSImport("getElementSize", Constants.LibName)]
    public static partial double[] js_get_element_size([JSMarshalAs<JSType.Any>] object htmlElement);

    [JSImport("listenResizeEvent", Constants.LibName)]
    [return: JSMarshalAs<JSType.Function>]
    public static partial Action js_listen_resize_event(
        [JSMarshalAs<JSType.Any>] object element, 
        [JSMarshalAs<JSType.Function>] Action resizeEvent);

    [JSImport("makeEventLoop", Constants.LibName)]
    [return: JSMarshalAs<JSType.Function>]
    public static partial Action js_make_event_loop(
        [JSMarshalAs<JSType.Function>] Action callback);
}