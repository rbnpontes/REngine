using System.Runtime.InteropServices.JavaScript;

namespace REngine.RHI.Web.Driver;

public static partial class DriverFactory
{
    [JSImport("_rengine_create_driver", Constants.LibName)]
    internal static partial void js_rengine_create_driver(IntPtr settingsPtr, IntPtr swapChainDesc,
        IntPtr nativeWindow, IntPtr driverResult);
}