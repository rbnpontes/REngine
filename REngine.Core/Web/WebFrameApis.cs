using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

#if WEB
public static partial class WebFrame
{
    [JSImport("_alert", WebLibConstants.LibName)]
    private static partial void js_alert(string message);
}
#endif