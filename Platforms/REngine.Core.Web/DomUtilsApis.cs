using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public static partial class DomUtils
{
    [JSImport("query_selector", Constants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object? js_query_selector(string selector);

    [JSImport("get_element_size", Constants.LibName)]
    private static partial double[] js_get_element_size(
        [JSMarshalAs<JSType.Any>] object element);

    [JSImport("on_resize_event", Constants.LibName)]
    [return: JSMarshalAs<JSType.Function>]
    private static partial Action js_on_resize_element(
        [JSMarshalAs<JSType.Any>] object element,
        [JSMarshalAs<JSType.Function>] Action resizeEvent
    );
}