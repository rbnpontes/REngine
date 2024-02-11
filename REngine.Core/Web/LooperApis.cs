using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public partial class WebLooper
{
#if WEB
    [JSImport("make_frame_loop", WebLibConstants.LibName)]
    [return: JSMarshalAs<JSType.Function>]
    private static partial Action js_make_frame_loop(
        [JSMarshalAs<JSType.Function>] Action callback);
#endif
}