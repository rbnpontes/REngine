using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public partial class JSArray
{
    [JSImport("array_new", Constants.LibName)]
    private static partial int js_array_new();

    [JSImport("array_free", Constants.LibName)]
    private static partial void js_array_free(int array);

    [JSImport("array_get_native", Constants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_array_get_native(int array);

    [JSImport("array_length", Constants.LibName)]
    private static partial int js_array_length(int array);

    [JSImport("array_push", Constants.LibName)]
    private static partial void js_array_push(
        int array,
        [JSMarshalAs<JSType.Any>] object item);

    [JSImport("array_get", Constants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_array_get(
        int array,
        int index);

    [JSImport("array_set", Constants.LibName)]
    private static partial void js_array_set(
        int array,
        int index,
        [JSMarshalAs<JSType.Any>] object value);

    [JSImport("array_clear", Constants.LibName)]
    private static partial void js_array_clear(int array);

    [JSImport("array_indexof", Constants.LibName)]
    private static partial int js_array_indexof(
        int array,
        [JSMarshalAs<JSType.Any>] object item);

    [JSImport("array_remove", Constants.LibName)]
    private static partial void js_array_insert(
        int array,
        int index,
        [JSMarshalAs<JSType.Any>] object item);
    
    [JSImport("array_remove", Constants.LibName)]
    private static partial void js_array_remove(
        int array,
        int index);
}