using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

public static partial class WebConsole
{
    [JSImport("console_log", Constants.LibName)]
    private static partial void js_console_log(int array);
    [JSImport("console_warn", Constants.LibName)]
    private static partial void js_console_warn(int array);
    [JSImport("console_error", Constants.LibName)]
    private static partial void js_console_error(int array);
}