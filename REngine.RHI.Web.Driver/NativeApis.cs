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

    [JSImport("read_i8", Constants.LibName)]
    public static partial byte js_read_i8(IntPtr addr);

    [JSImport("read_i16", Constants.LibName)]
    public static partial short js_read_i16(IntPtr addr);

    [JSImport("read_i32", Constants.LibName)]
    public static partial int js_read_i32(IntPtr addr);

    [JSImport("read_float", Constants.LibName)]
    public static partial float js_read_float(IntPtr addr);
    
    [JSImport("read_double", Constants.LibName)]
    public static partial float js_read_double(IntPtr addr);

    [JSImport("write_i8", Constants.LibName)]
    public static partial void js_write_i8(IntPtr addr, byte value);
    
    [JSImport("write_i16", Constants.LibName)]
    public static partial void js_write_i16(IntPtr addr, short value);
    
    [JSImport("write_i32", Constants.LibName)]
    public static partial void js_write_i32(IntPtr addr, int value);

    [JSImport("write_float", Constants.LibName)]
    public static partial void js_write_float(IntPtr addr, float value);
    
    [JSImport("write_double", Constants.LibName)]
    public static partial void js_write_double(IntPtr addr, double value);

    [JSImport("get_ptr_size", Constants.LibName)]
    public static partial int js_get_ptr_size();

    [JSImport("memcpy", Constants.LibName)]
    private static partial void js_memcpy_v0([JSMarshalAs<JSType.MemoryView>] Span<byte> src, IntPtr dst, int @sizeof);

    [JSImport("memcpy", Constants.LibName)]
    private static partial void js_memcpy_v1(IntPtr src, [JSMarshalAs<JSType.MemoryView>] Span<byte> dst, int @sizeof);

    [JSImport("memcpy", Constants.LibName)]
    private static partial void js_memcpy_v2(IntPtr src, IntPtr dst, int @sizeof);

    [JSImport("memset", Constants.LibName)]
    public static partial void js_memset(IntPtr src, int value, int @sizeof);

    [JSImport("get_last_method_args", Constants.LibName)]
    public static partial int[] js_get_last_method_v0();

    [JSImport("register_function", Constants.LibName)]
    public static partial IntPtr js_register_function([JSMarshalAs<JSType.Function>] Action callback, string signature);

    [JSImport("unregister_function", Constants.LibName)]
    public static partial void js_unregister_function(IntPtr functPtr);

    [JSImport("get_string", Constants.LibName)]
    public static partial string js_get_string(IntPtr ptr);

    [JSImport("alloc_string", Constants.LibName)]
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
}