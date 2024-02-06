namespace REngine.Core.Web;

public sealed partial class Looper : IDisposable
{
    private readonly Action pCall;
    private bool pDisposed;

    private Looper(Action call)
    {
        pCall = call;
    }
    
    public void Dispose()
    {
        if(pDisposed)
            return;
        pDisposed = true;
        pCall();
    }

    public static Looper Build(Action callback)
    {
        return new Looper(js_make_frame_loop(callback));
    }
}