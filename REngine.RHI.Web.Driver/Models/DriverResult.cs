namespace REngine.RHI.Web.Driver.Models;

internal class DriverResult : IDisposable
{
    public IntPtr Driver { get; private set; } = IntPtr.Zero;
    public IntPtr SwapChain { get; private set; } = IntPtr.Zero;
    public string Error { get; private set; } = string.Empty;
    public IntPtr Handle { get; private set; } = NativeApis.js_malloc(3 * NativeApis.js_get_ptr_size());

    public unsafe void Load()
    {
        var data = new int[3];
        fixed(void* ptr = data)
            NativeApis.js_memcpy(Handle, ptr, 3 * sizeof(int));
        
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