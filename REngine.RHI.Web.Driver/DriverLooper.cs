namespace REngine.RHI.Web.Driver;

public sealed class DriverLooper : IDisposable
{
    private readonly Action pDisposeCallback;
    private bool pDisposed;
    
    internal DriverLooper(Action disposeCallback)
    {
        pDisposeCallback = disposeCallback;
    }
    
    public void Dispose()
    {
        if (pDisposed)
            return;
        pDisposed = true;
        pDisposeCallback();
    }

    public static DriverLooper Build(Action callback)
    {
        return new DriverLooper(NativeApis.js_make_event_loop(callback));
    }
}