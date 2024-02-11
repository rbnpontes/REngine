namespace REngine.Core.Web;

public sealed partial class WebLooper : IDisposable
{
    private Action? pCall;
    private bool pDisposed;

    private WebLooper()
    {
    }
    
    public void Dispose()
    {
        if(pDisposed)
            return;
        pDisposed = true;
        pCall?.Invoke();
    }

    private void SetDisposeCall(Action action)
    {
        pCall = action;
    }

    public static WebLooper Build(Action<WebLooper> callback)
    {
#if WEB
        var looper = new WebLooper();
        var disposeCall = js_make_frame_loop(() => callback(looper));
        looper.SetDisposeCall(disposeCall);
        return looper;
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }
}