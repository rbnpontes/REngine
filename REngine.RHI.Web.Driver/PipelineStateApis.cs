using System.Runtime.InteropServices.JavaScript;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace REngine.RHI.Web.Driver;

internal partial class PipelineStateImpl
{
    [JSImport("_rengine_pipelinestate_createresourcebinding", Constants.LibName)]
    private static partial void js_rengine_pipelinestate_createresourcebinding(
        IntPtr pipeline,
        IntPtr result);
}