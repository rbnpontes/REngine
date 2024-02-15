using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;

namespace REngine.RHI.Web.Driver;

internal partial class CommandBufferImpl
{
    [JSImport("_rengine_cmdbuffer_setrts", Constants.LibName)]
    public static partial IntPtr js_rengine_cmdbuffer_setrts(
        IntPtr handle,
        IntPtr renderTargets,
        int numRenderTargets,
        IntPtr depthStencil,
        int isDeferred);

    [JSImport("_rengine_cmdbuffer_clearrt", Constants.LibName)]
    public static partial IntPtr js_rengine_cmdbuffer_clearrt(
        IntPtr handle,
        IntPtr rtHandle,
        IntPtr colorPtr,
        int isDeferred);

    [JSImport("_rengine_cmdbuffer_cleardepth", Constants.LibName)]
    public static partial void js_rengine_cmdbuffer_cleardepth(
        IntPtr handle,
        IntPtr depthStencil,
        int clearFlags,
        float depth,
        int stencil,
        int isDeferred);

    [JSImport("_rengine_cmdbuffer_setpipeline", Constants.LibName)]
    public static partial void js_rengine_cmdbuffer_setpipeline(IntPtr handle, IntPtr pipelineStateHandle);

    [JSImport("_rengine_cmdbuffer_setvbuffer", Constants.LibName)]
    public static partial void js_rengine_cmdbuffer_setvbuffer(
        IntPtr handle, int startSlot, int numBuffers,
        IntPtr buffers, IntPtr offsets, int reset, int isDeferred);

    [JSImport("_rengine_cmdbuffer_setibuffer", Constants.LibName)]
    public static partial void js_rengine_cmdbuffer_setibuffer(
        IntPtr handle, IntPtr indexBuffer, int byteOffset, int isDeferredByte);

    [JSImport("_rengine_cmdbuffer_commitbindings", Constants.LibName)]
    public static partial void js_rengine_cmdbuffer_commitbindings(
        IntPtr handle, IntPtr shaderResource, int isDeferred);

    [JSImport("_rengine_cmdbuffer_draw", Constants.LibName)]
    public static partial void js_rengine_cmdbuffer_draw(
        IntPtr handle, IntPtr drawArgs);

    [JSImport("_rengine_cmdbuffer_drawindexed",Constants.LibName)]
    public static partial void js_rengine_cmdbuffer_drawindexed(
        IntPtr handle, IntPtr drawArgs);

    [JSImport("_rengine_cmdbuffer_setblendfactors", Constants.LibName)]
    public static partial void js_rengine_cmdbuffer_setblendfactors(
        IntPtr handle, double r, double g, double b, double a);

    [JSImport("_rengine_cmdbuffer_setviewports", Constants.LibName)]
    public static partial void js_rengine_cmdbuffer_setviewports(
        IntPtr handle, IntPtr viewportsPtr, int viewportsLength,
        int rtWidth, int rtHeight, int isDeferred);

    [JSImport("_rengine_cmdbuffer_setscissors", Constants.LibName)]
    public static partial void js_rengine_cmdbuffer_setscissors(
        IntPtr handle, IntPtr scissorsPtr, int numScissors,
        int rtWidth, int rtHeight);

    [JSImport("_rengine_cmdbuffer_map", Constants.LibName)]
    public static partial IntPtr js_rengine_cmdbuffer_map(
        IntPtr handle,
        IntPtr buffer,
        int mapType,
        int mapFlags);

    [JSImport("_rengine_cmdbuffer_unmap", Constants.LibName)]
    public static partial void js_rengine_cmdbuffer_unmap(
        IntPtr handle,
        IntPtr buffer,
        int mapType);
}