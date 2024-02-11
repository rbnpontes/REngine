using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public partial class JSArray
{
#if WEB
    [JSImport("array_new", WebLibConstants.LibName)]
    private static partial int js_array_new();

    [JSImport("array_free", WebLibConstants.LibName)]
    private static partial void js_array_free(int array);

    [JSImport("array_get_native", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_array_get_native(int array);

    [JSImport("array_length", WebLibConstants.LibName)]
    private static partial int js_array_length(int array);

    [JSImport("array_push", WebLibConstants.LibName)]
    private static partial void js_array_push(
        int array,
        [JSMarshalAs<JSType.Any>] object item);

    [JSImport("array_get", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object js_array_get(
        int array,
        int index);

    [JSImport("array_set", WebLibConstants.LibName)]
    private static partial void js_array_set(
        int array,
        int index,
        [JSMarshalAs<JSType.Any>] object value);

    [JSImport("array_clear", WebLibConstants.LibName)]
    private static partial void js_array_clear(int array);

    [JSImport("array_indexof", WebLibConstants.LibName)]
    private static partial int js_array_indexof(
        int array,
        [JSMarshalAs<JSType.Any>] object item);

    [JSImport("array_remove", WebLibConstants.LibName)]
    private static partial void js_array_insert(
        int array,
        int index,
        [JSMarshalAs<JSType.Any>] object item);
    
    [JSImport("array_remove", WebLibConstants.LibName)]
    private static partial void js_array_remove(
        int array,
        int index);
#endif
}