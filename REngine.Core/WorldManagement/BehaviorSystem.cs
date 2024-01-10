using REngine.Core.Threading;

namespace REngine.Core.WorldManagement;

[Flags]
public enum BehaviorSystemEventFlags
{
    None =0,
    BeginUpdate = 1 << 0,
    Update = 1 << 1,
    EndUpdate = 1 << 2,
    All = BeginUpdate | Update | EndUpdate
}
public abstract class BehaviorSystem<T> : BaseSystem<T>, IDisposable where T : struct
{
    protected readonly IExecutionPipeline mExecutionPipeline;
    protected readonly EngineEvents mEngineEvents;
    private readonly BehaviorSystemEventFlags pEventFlags;

    private bool pDisposed;

    protected BehaviorSystem(
        IExecutionPipeline executionPipeline,
        EngineEvents engineEvents,
        BehaviorSystemEventFlags eventFlags
    )
    {
        mExecutionPipeline = executionPipeline;
        mEngineEvents = engineEvents;
        pEventFlags = eventFlags;
        
        engineEvents.OnStart += HandleEngineStart;
        engineEvents.OnBeforeStop += OnEngineStop;
    }

    private void HandleEngineStart(object? sender, EventArgs e)
    {
        mEngineEvents.OnStart -= HandleEngineStart;
        
        if((pEventFlags & BehaviorSystemEventFlags.BeginUpdate) != 0)
            mExecutionPipeline.AddEvent(DefaultEvents.UpdateBeginId, HandleUpdateBegin);
        if ((pEventFlags & BehaviorSystemEventFlags.Update) != 0)
            mExecutionPipeline.AddEvent(DefaultEvents.UpdateId, HandleUpdate);
        if((pEventFlags & BehaviorSystemEventFlags.EndUpdate) != 0)
            mExecutionPipeline.AddEvent(DefaultEvents.UpdateEndId, HandleUpdateEnd);
        OnEngineStart();
    }

    private void OnEngineStop(object? sender, EventArgs e)
    {
        mEngineEvents.OnBeforeStop -= OnEngineStop;
        Dispose();
    }

    private void HandleUpdateBegin(IExecutionPipeline _) => OnBeginUpdate();
    private void HandleUpdate(IExecutionPipeline _) => OnUpdate();
    private void HandleUpdateEnd(IExecutionPipeline _) => OnEndUpdate();

    public void Dispose()
    {
        if (pDisposed)
            return;
        pDisposed = true;

        mExecutionPipeline
            .RemoveEvent(DefaultEvents.UpdateBeginId, HandleUpdateBegin)
            .RemoveEvent(DefaultEvents.UpdateId, HandleUpdate)
            .RemoveEvent(DefaultEvents.UpdateEndId, HandleUpdateEnd);
        OnDispose();
        
        GC.SuppressFinalize(this);
    }
    
    protected virtual void OnDispose(){}
    
    protected virtual void OnEngineStart() {}
    protected virtual void OnBeginUpdate()
    {
    }

    protected virtual void OnUpdate()
    {
    }

    protected virtual void OnEndUpdate()
    {
    }

    protected override int GetExpansionSize()
    {
        return 1;
    }
}