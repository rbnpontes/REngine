using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal unsafe struct GraphicsDriverSettingsDto()
{
#if DEBUG
    public bool EnableValidation = true;
#else
    public bool EnableValidation = false;
#endif
    public uint GraphicsBackend = 5;
    public uint AdapterId = uint.MaxValue;
    public uint NumDeferredCtx = 0;
    public IntPtr MessageCallback = IntPtr.Zero;
    
    public ref GraphicsDriverSettingsDto GetPinnableReference()
    {
        return ref this;
    }
}