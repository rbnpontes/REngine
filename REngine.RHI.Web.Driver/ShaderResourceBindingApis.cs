using System.Runtime.InteropServices.JavaScript;

namespace REngine.RHI.Web.Driver;

internal partial class ShaderResourceBindingImpl
{
    [JSImport("_rengine_srb_set", Constants.LibName)]
    private static partial void js_rengine_srb_set(
        IntPtr srb,
        int flags,
        IntPtr resourceName,
        IntPtr resource);
}