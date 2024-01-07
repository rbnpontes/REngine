using System.Collections.Concurrent;

namespace REngine.Core;

/// <summary>
/// Use Disposable Queue if you wan't to dispose
/// objects on main thread. This prevents deadlock issues
/// </summary>
public static class DisposableQueue
{
    private static readonly ConcurrentQueue<IDisposable> sDisposables = new();

    public static void Enqueue(IDisposable? disposable)
    {
        if(disposable is not null)
            sDisposables.Enqueue(disposable);
    }

    public static void Dispose()
    {
        while(sDisposables.TryDequeue(out var disposable))
            disposable.Dispose();
    }
}