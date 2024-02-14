using System.Runtime.InteropServices.JavaScript;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace REngine.RHI.Web.Driver;

internal partial class BufferImpl
{
    [JSImport("_rengine_buffer_get_state", Constants.LibName)]
    private static partial int js_rengine_buffer_get_state(IntPtr handle);
    [JSImport("_rengine_buffer_set_state", Constants.LibName)]
    private static partial void js_rengine_buffer_set_state(IntPtr handle, int resourceState);
    [JSImport("_rengine_buffer_get_gpuhandle", Constants.LibName)]
    private static partial int js_rengine_buffer_get_gpuhandle(IntPtr handle);
}