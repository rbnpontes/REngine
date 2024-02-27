using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public static partial class WebConsole
{
#if WEB
    [JSImport("console_debug", WebLibConstants.LibName)]
    private static partial void js_console_debug(int array);
    [JSImport("console_log", WebLibConstants.LibName)]
    private static partial void js_console_log(int array);
    [JSImport("console_warn", WebLibConstants.LibName)]
    private static partial void js_console_warn(int array);
    [JSImport("console_error", WebLibConstants.LibName)]
    private static partial void js_console_error(int array);
#endif
}