using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public partial class Looper
{
    [JSImport("make_frame_loop", Constants.LibName)]
    [return: JSMarshalAs<JSType.Function>]
    private static partial Action js_make_frame_loop(
        [JSMarshalAs<JSType.Function>] Action callback);
}