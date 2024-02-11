using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public partial class JSObject
{
#if WEB
    [JSImport("object_get_prop", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object? js_object_get_prop(
        [JSMarshalAs<JSType.Any>] object obj,
        string propKey);

    [JSImport("object_get_prop_func", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Function>]
    private static partial Action js_object_get_prop_func(
        [JSMarshalAs<JSType.Any>] object obj,
        string propKey);

    [JSImport("object_set_prop", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_object_set_prop(
        [JSMarshalAs<JSType.Any>] object obj,
        string propKey,
        [JSMarshalAs<JSType.Any>] object value);

    [JSImport("object_set_prop", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_object_set_prop_str(
        [JSMarshalAs<JSType.Any>] object obj,
        string propKey,
        string value);

    [JSImport("object_set_prop", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_object_set_prop_int(
        [JSMarshalAs<JSType.Any>] object obj,
        string propKey,
        int value);

    [JSImport("object_set_prop", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_object_set_prop_double(
        [JSMarshalAs<JSType.Any>] object obj,
        string propKey,
        double value);
#endif
}