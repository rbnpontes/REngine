namespace REngine.Core.Threading;

public interface IDispatcher : IDisposable
{
    public bool IsThreadCaller { get; }
    public void Run();
    public void Invoke(Action action);
    public Task InvokeAsync(Action action);
    public Task Yield();
}