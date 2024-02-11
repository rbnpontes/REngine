using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public static partial class DomUtils
{
#if WEB
    [JSImport("query_selector", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object? js_query_selector(string selector);

    [JSImport("request_fullscreen", WebLibConstants.LibName)]
    private static partial void js_request_fullscreen(
        [JSMarshalAs<JSType.Any>] object element);

    [JSImport("exit_fullscreen", WebLibConstants.LibName)]
    private static partial void js_exit_fullscreen();

    [JSImport("is_fullscreen", WebLibConstants.LibName)]
    private static partial bool js_is_fullscreen();
    
    [JSImport("get_element_size", WebLibConstants.LibName)]
    private static partial double[] js_get_element_size(
        [JSMarshalAs<JSType.Any>] object element);

    [JSImport("get_element_bounds", WebLibConstants.LibName)]
    private static partial double[] js_get_element_bounds(
        [JSMarshalAs<JSType.Any>] object element);
    
    [JSImport("set_element_size", WebLibConstants.LibName)]
    private static partial void js_set_element_size(
        [JSMarshalAs<JSType.Any>] object element,
        double width, double height);
    
    [JSImport("on_resize_event", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Function>]
    private static partial Action js_on_resize_element(
        [JSMarshalAs<JSType.Any>] object element,
        [JSMarshalAs<JSType.Function>] Action resizeEvent
    );

    [JSImport("element_get_max_size", WebLibConstants.LibName)]
    private static partial double[] js_element_get_max_size(
        [JSMarshalAs<JSType.Any>] object element);

    [JSImport("element_set_max_size", WebLibConstants.LibName)]
    private static partial void js_element_set_max_size(
        [JSMarshalAs<JSType.Any>] object element, double width, double height);

    [JSImport("element_get_min_size", WebLibConstants.LibName)]
    private static partial double[] js_element_get_min_size(
        [JSMarshalAs<JSType.Any>] object element);

    [JSImport("element_set_min_size", WebLibConstants.LibName)]
    private static partial void js_element_set_min_size(
        [JSMarshalAs<JSType.Any>] object element,
        double width, double height);
#endif
}