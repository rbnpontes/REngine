using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public sealed partial class HTMLElement
{
    [JSImport("get_element_parent", Constants.LibName)]
    [return: JSMarshalAs<JSType.Any>]
    private static partial object? js_get_element_parent(
        [JSMarshalAs<JSType.Any>] object element);
}