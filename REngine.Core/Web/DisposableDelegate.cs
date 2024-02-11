namespace REngine.Core.Web;

internal class DisposableDelegate(Action action) : IDisposable
{
    private bool pDisposed = false;
    public void Dispose()
    {
        if (pDisposed)
            return;
        pDisposed = true;
        action();
    }
}