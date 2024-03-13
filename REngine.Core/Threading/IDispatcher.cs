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
    /// <summary>
    /// If current thread is the same thread
    /// of Dispatcher, the return will be true
    /// </summary>
    public bool IsThreadCaller { get; }
    /// <summary>
    /// Run Dispatcher, generally this operation is thread blocking
    /// </summary>
    public void Run();
    /// <summary>
    /// Schedule a Action to be invoked by Dispatcher
    /// </summary>
    /// <param name="action"></param>
    public void Invoke(Action action);
    /// <summary>
    /// Schedule a Action to be invoked by Dispatcher
    /// Then return an awaitable task.
    /// NOTE: Don't reuse task. Task is always reused by dispatcher
    /// So its not safe to be reused.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public IDispatcherTask InvokeAsync(Action action);
    /// <summary>
    /// Schedule an Opaque Action to be invoke by Dispatcher
    /// Then return an awaitable task.
    /// NOTE: Don't reuse task. Task is always reused by dispatcher
    /// So its not safe to be reused.
    /// </summary>
    /// <returns></returns>
    public IDispatcherTask Yield();
}