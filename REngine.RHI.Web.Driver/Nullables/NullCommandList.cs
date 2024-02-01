namespace REngine.RHI.Web.Driver;

internal sealed class NullCommandList : ICommandList
{
    public void Dispose()
    {
    }

    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed => false;
    public event EventHandler? OnDispose;
    public GPUObjectType ObjectType => GPUObjectType.Unknown;
    public string Name => "Null Command List";

    public static readonly ICommandList Instance = new NullCommandList();
}