using REngine.Core;

namespace REngine.RHI.Web.Driver;

internal class UndisposableNativeObject(IntPtr handle) : INativeObject
{
    public IntPtr Handle => handle;
    public bool IsDisposed => false;
    public event EventHandler? OnDispose;
    public void Dispose()
    {
    }
}