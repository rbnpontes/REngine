using System.Runtime.InteropServices.JavaScript;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace REngine.RHI.Web.Driver;

internal partial class DeviceImpl
{
    [JSImport("_rengine_device_create_shader", Constants.LibName)]
    private static partial void js_rengine_device_create_shader(
        IntPtr handle, IntPtr shaderCiPtr, IntPtr resultPtr);

    [JSImport("_rengine_device_create_graphicspipeline", Constants.LibName)]
   private static partial void js_rengine_device_create_graphicspipeline(
        IntPtr handle, IntPtr graphicsPipelineDescPtr, byte isOpenGl, IntPtr result);

    [JSImport("_rengine_device_create_texture", Constants.LibName)]
    private static partial void js_rengine_device_create_texture(
        IntPtr handle,
        IntPtr desc,
        IntPtr data,
        int numTexData,
        IntPtr result);

    [JSImport("_rengine_device_create_buffer", Constants.LibName)]
    private static partial void js_rengine_device_create_buffer(
        IntPtr handle,
        IntPtr desc,
        int size,
        IntPtr data,
        IntPtr result);
}