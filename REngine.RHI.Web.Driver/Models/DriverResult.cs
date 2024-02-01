namespace REngine.RHI.Web.Driver.Models;

internal class DriverResult : IDisposable
{
    public IntPtr Driver { get; private set; } = IntPtr.Zero;
    public IntPtr SwapChain { get; private set; } = IntPtr.Zero;
    public string Error { get; private set; } = string.Empty;
    public IntPtr Handle { get; private set; } = NativeApis.js_malloc(3 * NativeApis.js_get_ptr_size());

    public void Load()
    {
        var data = new int[3];
        var span = data.AsSpan();
        NativeApis.js_memcpy(Handle, span, 3);
        
        Driver = new IntPtr(data[0]);
        SwapChain = new IntPtr(data[1]);
        Error = NativeApis.js_get_string(data[2]);
    }
    
    public void Dispose()
    {
        NativeApis.js_free(Handle);
        Handle = IntPtr.Zero;
    }
}