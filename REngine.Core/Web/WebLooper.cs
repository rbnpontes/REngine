using REngine.Core.Exceptions;
using REngine.Core.IO;

namespace REngine.Core.Web;

public sealed partial class WebLooper : IDisposable
{
    private readonly Action<WebLooper> pCallback;
    private readonly ILogger<WebLooper> pLogger;
    private readonly Action pDisposeCall;
    private bool pDisposed;

    private WebLooper(Action<WebLooper> callback, ILogger<WebLooper> logger)
    {
        pCallback = callback;
        pLogger = logger;
#if WEB
        pDisposeCall = js_make_frame_loop(ExecuteLoop);
#endif
    }
    
    public void Dispose()
    {
        if(pDisposed)
            return;
        pDisposed = true;
        pLogger.Debug("Stopping Looper");
        pDisposeCall.Invoke();
    }
    
    private void ExecuteLoop()
    {
        try
        {
            pCallback(this);
        }
        catch(Exception ex)
        {
            pLogger.Error("Uncaught Exception:", ex.GetFullString());
            Dispose();
#if DEBUG
            WebFrame.Alert("Uncaught Exception on Looper. See Logs!");
#endif
        }
    }

    public static WebLooper Build(Action<WebLooper> callback, ILoggerFactory loggerFactory)
    {
#if WEB
        return new WebLooper(callback, loggerFactory.Build<WebLooper>());
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }
}