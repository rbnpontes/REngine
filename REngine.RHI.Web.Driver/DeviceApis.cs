using System.Runtime.InteropServices.JavaScript;

namespace REngine.RHI.Web.Driver;

internal partial class DeviceImpl
{
    [JSImport("_rengine_device_create_shader", Constants.LibName)]
    public static partial void js_rengine_device_create_shader(
        IntPtr handle, IntPtr shaderCiPtr, IntPtr resultPtr);

    [JSImport("_rengine_device_create_graphicspipeline", Constants.LibName)]
    public static partial void js_rengine_device_create_graphicspipeline(
        IntPtr handle, IntPtr graphicsPipelineDescPtr, byte isOpenGl, IntPtr result);

}