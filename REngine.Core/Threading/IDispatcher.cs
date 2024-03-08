using System.Runtime.CompilerServices;

namespace REngine.Core.Threading;

public interface IDispatcherAwaiter : INotifyCompletion
{
    public bool IsCompleted { get; }
    public void GetResult();
}

public interface IDispatcherTask
{
    public IDispatcherAwaiter GetAwaiter();
}
public interface IDispatcher : IDisposable
{
    public bool IsThreadCaller { get; }
    public void Run();
    public void Invoke(Action action);
    public IDispatcherTask InvokeAsync(Action action);
    public IDispatcherTask Yield();
}