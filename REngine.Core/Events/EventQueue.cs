namespace REngine.Core.Events;

public sealed class EventQueue<T>
{
    private readonly object pSync = new();
    private readonly List<Action<object?, T>> pActions = new();

    public void Add(Action<object?, T> eventCallback)
    {
        lock(pSync)
            pActions.Add(eventCallback);
    }

    public void Remove(Action<object?, T> eventCallback)
    {
        lock (pSync)
            pActions.Remove(eventCallback);
    }

    public void Invoke(object? sender, T eventArgs)
    {
        Action<object, T> call;
        int totalItems;
        var idx = 0;

        lock (pSync)
            totalItems = pActions.Count;
        while (idx < totalItems)
        {
            lock (pSync)
            {
                totalItems = pActions.Count;
                call = pActions[idx];
            }

            ++idx;
            call(sender, eventArgs);
        }
    }
}

public sealed class EventQueue
{
    private readonly object pSync = new();
    private readonly List<Action<object?>> pActions = new();
    
    public void Add(Action<object?> eventCallback)
    {
        lock(pSync)
            pActions.Add(eventCallback);
    }

    public void Remove(Action<object?> eventCallback)
    {
        lock (pSync)
            pActions.Remove(eventCallback);
    }

    public void ClearAllListeners()
    {
        lock (pSync)
            pActions.Clear();
    }

    public void Invoke(object? sender)
    {
        Action<object?> call;
        int totalItems;
        var idx = 0;

        lock (pSync)
            totalItems = pActions.Count;
        while (idx < totalItems)
        {
            lock (pSync)
            {
                totalItems = pActions.Count;
                call = pActions[idx];
            }

            ++idx;
            call(sender);
        }
    }
}