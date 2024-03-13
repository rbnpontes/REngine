using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using REngine.Core.IO;

namespace REngine.Core.Threading;

public class DefaultDispatcher : IDispatcher
{
    private class DispatcherAwaiter : IDispatcherAwaiter
    {
        private Action? pContinuation;
        public Action? AuxiliarAction { get; set; }
        public bool IsCompleted { get; private set; }

        private void ExecuteContinuation()
        {
            AuxiliarAction?.Invoke();
            pContinuation?.Invoke();
            IsCompleted = true;
        }
        public bool Execute()
        {
            if (pContinuation is null)
                return false;
            ExecuteContinuation();
            return true;
        }
        public void OnCompleted(Action continuation)
        {
            pContinuation = continuation;
        }

        public void GetResult() {}
        public void Reset()
        {
            IsCompleted = false;
            pContinuation = AuxiliarAction = null;
        }
    }
    private class DispatcherTask() : IDispatcherTask
    {
        public readonly DispatcherAwaiter Awaiter = new();
        public bool IsCompleted => Awaiter.IsCompleted;
        public IDispatcherAwaiter GetAwaiter() => Awaiter;

        public void Reset()
        {
            Awaiter.Reset();
        }
    }

    private readonly Queue<DispatcherTask>[] pTasksContainer = new[]
    {
        new Queue<DispatcherTask>(),
        new Queue<DispatcherTask>()
    };

    private readonly ConcurrentQueue<DispatcherTask> pAvailableTasks = new();
    private readonly ILogger<IDispatcher>? pLogger;
    private readonly object pSync = new();
    private readonly int pThreadId;
    
    private bool pDisposed;
    private bool pIsRunning;

    private int pExecutionContainerIdx;
    
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
        lock (pSync)
            pDisposed = true;
        
        // Execute incoming tasks
        foreach (var _ in pTasksContainer)
            ExecuteQueue();
        GC.SuppressFinalize(this);
    }

    private void ExecuteQueue()
    {
        int idx;
        lock (pSync)
            idx = pExecutionContainerIdx;

        var numOfTasks = pTasksContainer[idx].Count;
        while (pTasksContainer[idx].TryDequeue(out var task))
        {
            // If task is not ready, schedule to next iteration
            if(!task.Awaiter.Execute())
                EnqueueTask(task);
            
            if (!task.IsCompleted) 
                continue;
            // reset and add completed task to be reused later
            task.Reset();
            pAvailableTasks.Enqueue(task);
        }

        // If at end of execution of tasks results in a count of
        // available tasks greater than executed tasks.
        // then we need to pop the difference between these
        // and let GC to free this objects to us.
        var diff = pAvailableTasks.Count - numOfTasks;
        for (var i = 0; i < diff; ++i)
        {
            if (!pAvailableTasks.TryDequeue(out var _))
                break;
        }
        
        lock (pSync)
        {
            ++idx;
            pExecutionContainerIdx = idx % pTasksContainer.Length;
        }
    }

    public void Run()
    {
        if(pIsRunning)
            return;
        if (pThreadId != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("You must run dispatcher on the same thread of the creation");
        
        pIsRunning = true;
        while (true)
        {
            lock (pSync)
            {
                if(pDisposed)
                    break;
            }
            
            ExecuteQueue();
        }

        pLogger?.Info("Dispatcher is Stopped. Exiting!");
    }

    private void EnqueueTask(DispatcherTask task)
    {
        lock (pSync)
            pTasksContainer[(pExecutionContainerIdx + 1) % pTasksContainer.Length].Enqueue(task);
    }

    private DispatcherTask AcquireTask()
    {
        return pAvailableTasks.TryDequeue(out var task) ? task : new DispatcherTask();
    }
    
    public void Invoke(Action action)
    {
        lock (pSync)
        {
            if (pDisposed)
                return;
        }

        var task = AcquireTask();
        task.GetAwaiter().OnCompleted(action);
        
        EnqueueTask(task);
    }

    public IDispatcherTask InvokeAsync(Action action)
    {
        var task = AcquireTask();
        task.Awaiter.AuxiliarAction = action;
        EnqueueTask(task);
        return task;
    }
    public IDispatcherTask Yield()
    {
        var task = AcquireTask();
        EnqueueTask(task);
        return task;
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
    private struct NullAwaiter : IDispatcherAwaiter
    {
        public void OnCompleted(Action continuation)
        {
            continuation();
        }

        public bool IsCompleted => true;
        public void GetResult()
        {
        }
    }
    private readonly struct NullTask() : IDispatcherTask
    {
        private readonly NullAwaiter pAwaiter = new();
        public IDispatcherAwaiter GetAwaiter() => pAwaiter;
    }
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

    public IDispatcherTask InvokeAsync(Action action)
    {
        action();
        return new NullTask();
    }

    public IDispatcherTask Yield()
    {
        return new NullTask();
    }

    public static readonly NullDispatcher Instance = new();
}