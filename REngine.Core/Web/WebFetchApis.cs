using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public static partial class WebFetch
{
#if WEB
    [JSImport("_fetch", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    private static partial Task<object> js_fetch(
        string url, string method);

    [JSImport("fetch_read_result", WebLibConstants.LibName)]
    private static partial void js_fetch_read_result(
        [JSMarshalAs<JSType.Any>] object result,
        [JSMarshalAs<JSType.MemoryView>] Span<byte> buffer);
#endif
}