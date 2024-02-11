using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public static partial class WebMarshal
{
#if WEB
    [JSImport("cast", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_cast_to_obj(string value);

    [JSImport("cast", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_cast_to_obj(int value);

    [JSImport("cast", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_cast_to_obj(double value);

    [JSImport("cast", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_cast_to_obj(float value);

    [JSImport("cast", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_cast_to_obj(bool value);

    [JSImport("cast", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_cast_to_obj(
        [JSMarshalAs<JSType.Function>] Action action);

    [JSImport("cast", WebLibConstants.LibName)]
    private static partial string js_cast_to_string(
        [JSMarshalAs<JSType.Any>] object obj);

    [JSImport("cast", WebLibConstants.LibName)]
    private static partial int js_cast_to_int(
        [JSMarshalAs<JSType.Any>] object obj);

    [JSImport("cast", WebLibConstants.LibName)]
    private static partial double js_cast_to_double(
        [JSMarshalAs<JSType.Any>] object obj);

    [JSImport("cast", WebLibConstants.LibName)]
    private static partial float js_cast_to_float(
        [JSMarshalAs<JSType.Any>] object obj);

    [JSImport("cast", WebLibConstants.LibName)]
    private static partial bool js_cast_to_bool(
        [JSMarshalAs<JSType.Any>] object obj);

    [JSImport("cast", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Function>]
    private static partial Action js_cast_to_action(
        [JSMarshalAs<JSType.Any>] object obj);

    [JSImport("_typeof", WebLibConstants.LibName)]
    private static partial string js_typeof(
        [JSMarshalAs<JSType.Any>] object obj);

    [JSImport("free_internal_memory", WebLibConstants.LibName)]
    private static partial void js_free_internal_memory();
#endif
}