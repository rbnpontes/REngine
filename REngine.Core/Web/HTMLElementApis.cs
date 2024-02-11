using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public sealed partial class HTMLElement
{
#if WEB
    [JSImport("get_element_parent", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object? js_get_element_parent(
        [JSMarshalAs<JSType.Any>] object element);

    [JSImport("get_element_attr", WebLibConstants.LibName)]
    private static partial string js_get_element_attr(
        [JSMarshalAs<JSType.Any>] object element, string key);

    [JSImport("set_element_attr", WebLibConstants.LibName)]
    private static partial void js_set_element_attr(
        [JSMarshalAs<JSType.Any>] object element,
        string key,
        string value);

    [JSImport("get_element_attrs", WebLibConstants.LibName)]
    private static partial string[] js_get_element_attrs(
        [JSMarshalAs<JSType.Any>] object element);

    [JSImport("element_add_event_listener", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Function>]
    private static partial Action js_element_add_event_listener(
        [JSMarshalAs<JSType.Any>] object element,
        string eventName,
        [JSMarshalAs<JSType.Function<JSType.Any>>] Action<object> callback);

    [JSImport("element_focus", WebLibConstants.LibName)]
    private static partial void js_element_focus(
        [JSMarshalAs<JSType.Any>] object element);
#endif
}