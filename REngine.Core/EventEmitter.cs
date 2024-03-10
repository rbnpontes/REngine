using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace REngine.Core;

public interface IEventEmitter : IDisposable
{
    public bool IsDisposed { get; }
    public int Listeners { get; }
    public void ClearAll();
}

public abstract class BaseEventEmitter<TListener> : IEventEmitter 
{
    protected readonly List<TListener> mListeners = new();
    protected readonly Queue<TListener> mOnceListeners = new();
    public bool IsDisposed { get; private set; }
    public int Listeners => mListeners.Count + mOnceListeners.Count;

    protected BaseEventEmitter()
    {
#if RENGINE_VALIDATIONS
        if (!typeof(TListener).IsSubclassOf(typeof(Delegate)))
            throw new InvalidCastException($"{nameof(TListener)} must be a {nameof(Delegate)} type of.");
#endif
    }
    
    ~BaseEventEmitter()
    {
        HandleDispose();
    }
    
    public void Dispose()
    {
        HandleDispose();
        GC.SuppressFinalize(this);
    }

    private void HandleDispose()
    {
        if(IsDisposed)
            return;
        IsDisposed = true;
        OnDispose();
        ClearAll();
    }

    public virtual void On(TListener listener)
    {
        mListeners.Add(listener);
    }

    public virtual void Off(TListener listener)
    {
        mListeners.Remove(listener);
    }

    public virtual void Once(TListener listener)
    {
        mOnceListeners.Enqueue(listener);
    }
    
    protected virtual void OnDispose() {}

    public virtual void ClearAll()
    {
        mListeners.Clear();
        mOnceListeners.Clear();
    }
}

public sealed class EventEmitter : BaseEventEmitter<Action<object>>
{
    private readonly Queue<Action<object>> pOnceListeners = new();
    public void Invoke(object sender)
    {
        while (mOnceListeners.TryDequeue(out var evtCall))
            evtCall(sender);
        
        var idx = 0;
        while (idx < mListeners.Count)
        {
            mListeners[idx](sender);
            ++idx;
        }
    }
    
    public override void Once(Action<object> listener)
    {
        pOnceListeners.Enqueue(listener);
    }

    public static EventEmitter operator +(EventEmitter emitter, Action<object> evtCall)
    {
        emitter.On(evtCall);
        return emitter;
    }
    public static EventEmitter operator -(EventEmitter emitter, Action<object> evtCall)
    {
        emitter.Off(evtCall);
        return emitter;
    }
}

public sealed class EventEmitter<T> : BaseEventEmitter<Action<object, T>>
{
    public void Invoke(object sender, T eventArg)
    {
        while (mOnceListeners.TryDequeue(out var action))
            action(sender, eventArg);
        
        var idx = 0;
        while (idx < mListeners.Count)
        {
            mListeners[idx](sender, eventArg);
            ++idx;
        }
    }

    public static EventEmitter<T> operator +(EventEmitter<T> emitter, Action<object, T> evtCall)
    {
        emitter.On(evtCall);
        return emitter;
    }
    public static EventEmitter<T> operator -(EventEmitter<T> emitter, Action<object, T> evtCall)
    {
        emitter.Off(evtCall);
        return emitter;
    }
}

public sealed class AsyncEventEmitter : BaseEventEmitter<Func<object, Task>>
{
    private readonly Queue<Task> pWaitQueue = new();
    public async Task Invoke(object sender)
    {
        while (mOnceListeners.TryDequeue(out var action))
            pWaitQueue.Enqueue(action(sender));
        
        var idx = 0;
        while (idx < mListeners.Count)
        {
            pWaitQueue.Enqueue(mListeners[idx](sender));
            ++idx;
        }

        // Wait Loaded Tasks
        while (pWaitQueue.TryDequeue(out var task))
            await task;
    }

    protected override void OnDispose()
    {
        pWaitQueue.Clear();
    }

    public static AsyncEventEmitter operator +(AsyncEventEmitter emitter, Func<object, Task> evtCall)
    {
        emitter.On(evtCall);
        return emitter;
    }
    public static AsyncEventEmitter operator -(AsyncEventEmitter emitter, Func<object, Task> evtCall)
    {
        emitter.Off(evtCall);
        return emitter;
    }
}

public sealed class AsyncEventEmitter<T> : BaseEventEmitter<Func<object, T, Task>>
{
    private readonly Queue<Task> pWaitQueue = new();
    public async Task Invoke(object sender, T eventArg)
    {
        while(mOnceListeners.TryDequeue(out var action))
            pWaitQueue.Enqueue(action(sender, eventArg));
        
        var idx = 0;
        while (idx < mListeners.Count)
        {
            pWaitQueue.Enqueue(mListeners[idx](sender, eventArg));
            ++idx;
        }

        // Wait Loaded Tasks
        while (pWaitQueue.TryDequeue(out var task))
            await task;
    }

    protected override void OnDispose()
    {
        pWaitQueue.Clear();
    }
    
    public static AsyncEventEmitter<T> operator +(AsyncEventEmitter<T> emitter, Func<object, T, Task> evtCall)
    {
        emitter.On(evtCall);
        return emitter;
    }
    public static AsyncEventEmitter<T> operator -(AsyncEventEmitter<T> emitter, Func<object, T, Task> evtCall)
    {
        emitter.Off(evtCall);
        return emitter;
    }
}

public sealed class ConcurrentEventEmitter : BaseEventEmitter<Action<object>>
{
    private readonly object pSync = new();
    public override void On(Action<object> listener)
    {
        lock(pSync)
            base.On(listener);
    }

    public override void Off(Action<object> listener)
    {
        lock(pSync)
            base.Off(listener);
    }

    public override void Once(Action<object> listener)
    {
        lock(pSync)
            base.Once(listener);
    }
    
    public override void ClearAll()
    {
        lock(pSync)
            base.ClearAll();
    }

    public void Invoke(object sender)
    {
        InvokeOnceListeners(sender);
        var idx = 0;
        int length;
        do
        {
            Action<object> listener;
            lock (pSync)
            {
                length = mListeners.Count;
                listener = mListeners[idx];
                ++idx;
            }

            listener(sender);
        } while (idx < length);
    }

    private void InvokeOnceListeners(object sender)
    {
        while (true)
        {
            Action<object>? eventCall;
            lock (pSync)
                mOnceListeners.TryDequeue(out eventCall);

            if (eventCall is null)
                break;
            eventCall(sender);
        }
    }
    
    public static ConcurrentEventEmitter operator +(ConcurrentEventEmitter emitter, Action<object> evtCall)
    {
        emitter.On(evtCall);
        return emitter;
    }
    public static ConcurrentEventEmitter operator -(ConcurrentEventEmitter emitter, Action<object> evtCall)
    {
        emitter.Off(evtCall);
        return emitter;
    }
}

public sealed class ConcurrentEventEmitter<T> : BaseEventEmitter<Action<object, T>>
{
    private readonly object pSync = new();
    public override void On(Action<object, T> listener)
    {
        lock(pSync)
            base.On(listener);
    }
    
    public override void ClearAll()
    {
        lock(pSync)
            base.ClearAll();
    }

    public void Invoke(object sender, T eventArg)
    {
        InvokeOnceListeners(sender, eventArg);
        var idx = 0;
        int length;
        do
        {
            Action<object, T> listener;
            lock (pSync)
            {
                length = mListeners.Count;
                listener = mListeners[idx];
                ++idx;
            }

            listener(sender, eventArg);
        } while (idx < length);
    }
    
    private void InvokeOnceListeners(object sender, T eventArg)
    {
        while (true)
        {
            Action<object, T>? eventCall;
            lock (pSync)
                mOnceListeners.TryDequeue(out eventCall);

            if (eventCall is null)
                break;
            eventCall(sender, eventArg);
        }
    }
    
    public static ConcurrentEventEmitter<T> operator +(ConcurrentEventEmitter<T> emitter, Action<object, T> evtCall)
    {
        emitter.On(evtCall);
        return emitter;
    }
    public static ConcurrentEventEmitter<T> operator -(ConcurrentEventEmitter<T> emitter, Action<object, T> evtCall)
    {
        emitter.Off(evtCall);
        return emitter;
    }
}