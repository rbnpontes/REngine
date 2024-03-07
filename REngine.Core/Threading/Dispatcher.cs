using System.Collections.Concurrent;
using REngine.Core.IO;

namespace REngine.Core.Threading;

public class DefaultDispatcher : IDispatcher
{
    private readonly ConcurrentQueue<Action> pDispatchActions = new();
    private readonly ILogger<IDispatcher>? pLogger;
    private readonly object pSync = new();
    private readonly int pThreadId;
    
    private bool pDisposed;
    private bool pIsRunning;

    public bool IsThreadCaller => pThreadId == Environment.CurrentManagedThreadId;
    private DefaultDispatcher(ILogger<IDispatcher>? logger)
    {
        pLogger = logger;
        pThreadId = Environment.CurrentManagedThreadId;
    }
    
    public void Dispose()
    {
        if (pDisposed)
            return;
        pLogger?.Debug("Disposing Dispatcher");
        lock(pSync)
            pDisposed = true;
        pDispatchActions.Clear();
        GC.SuppressFinalize(this);
    }

    public void Run()
    {
        if(pIsRunning)
            return;
        if (pThreadId != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("You must run dispatcher on the same thread of the creation");
        pIsRunning = true;
        var stop = false;
        while (!stop)
        {
            lock (pSync)
                stop = pDisposed;
            while (pDispatchActions.TryDequeue(out var action))
            {
                if (stop)
                    break;
                
                action();
                
                lock (pSync)
                    stop = pDisposed;
            }
        }

        pLogger?.Info("Dispatcher is Stopped. Exiting!");
    }

    public void Invoke(Action action)
    {
        lock (pSync)
        {
            if (pDisposed)
                return;
        }

        // if invoke is called on the same thread of dispatcher.
        // then executes instead of scheduling
        if (pThreadId == Environment.CurrentManagedThreadId)
            action();
        else
            pDispatchActions.Enqueue(action);
    }

    public Task InvokeAsync(Action action)
    {
        TaskCompletionSource completionSource = new();
        Invoke(()=>
        {
            action();
            completionSource.SetResult();
        });
        return completionSource.Task;
    }
    public Task Yield()
    {
        TaskCompletionSource completionSource = new();
        Invoke(()=> completionSource.SetResult());
        return completionSource.Task;
    }
    /// <summary>
    /// Build a DefaultDispatcher
    /// The dispatcher will be linked by the thread caller
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static IDispatcher Build(ILoggerFactory? factory)
    {
        return new DefaultDispatcher(factory?.Build<IDispatcher>());
    }
}

public class NullDispatcher : IDispatcher
{
    private NullDispatcher() {}
    public bool IsThreadCaller => true;
    
    public void Dispose()
    {
    }

    public void Run()
    {
    }

    public void Invoke(Action action)
    {
        action();
    }

    public Task InvokeAsync(Action action)
    {
        TaskCompletionSource<bool> src = new(false);
        action();
        src.SetResult(true);
        return src.Task;
    }

    public Task Yield()
    {
        TaskCompletionSource<bool> src = new(false);
        src.SetResult(true);
        return src.Task;
    }

    public static readonly NullDispatcher Instance = new();
}