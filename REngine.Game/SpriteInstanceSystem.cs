using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using REngine.Core;
using REngine.Core.Exceptions;
using REngine.Core.IO;
using REngine.Core.Threading;
using REngine.Core.WorldManagement;
using REngine.Game.Components;
using REngine.RPI;
using REngine.RPI.Batches;
using REngine.RPI.Effects;

namespace REngine.Game;

public enum SpriteInstanceType
{
    Default =0,
    Dynamic,
    Static
}
public struct SpriteInstanceData()
{
    public bool Enabled;
    public bool Dirty;
    public SpriteInstanceType Type;
    public SpriteBatchItem[] Items;
    public Transform2DSnapshot LastTransformSnapshot;
    
    public SpriteEffect? Effect;
    public SpriteInstanceBatch? Batch;

    public WeakReference<Transform2D> Transform = new(null);

    public SpriteInstanceComponent? Ref = null;
}

public sealed class SpriteInstanceSystem: BehaviorSystem<SpriteInstanceData>
{
    private readonly object pSync = new();
    private readonly ILogger<SpriteInstanceSystem> pLogger;
    private readonly ConcurrentQueue<int> pComponents2Update = new();
    
    private readonly ISpriteBatch pSpriteBatch;

    public SpriteInstanceSystem(
        IExecutionPipeline executionPipeline,
        EngineEvents engineEvents,
        ISpriteBatch spriteBatch,
        ILoggerFactory loggerFactory) : base(executionPipeline, engineEvents, BehaviorSystemEventFlags.None)
    {
        pSpriteBatch = spriteBatch;
        pLogger = loggerFactory.Build<SpriteInstanceSystem>();
    }

    public int Create(SpriteInstanceComponent component)
    {
        lock (pSync)
        {
            var id = Acquire();
            pData[id] = new SpriteInstanceData()
            {
                Ref = component,
                Dirty = true
            };
            return id;
        }
    }

    public void Destroy(int id)
    {
        lock (pSync)
        {
            if (pData[id].Ref is null || pAvailableIdx.Count == pData.Length)
                return;

            pData[id].Ref = null;
            DisposableQueue.Enqueue(pData[id].Batch);
            pAvailableIdx.Enqueue(id);
        }
    }

    public Transform2D GetTransform(int id)
    {
        Entity? owner;
        Transform2D? transform;
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            var component = pData[id].Ref;
            pData[id].Transform.TryGetTarget(out transform);
            owner = component?.Owner;
        }

        if (owner is null)
            throw new NullReferenceException("Component is not already initialized");
        if (transform is not null)
        {
            if (transform.IsDisposed || transform.Owner != owner)
                transform = null;
        }

        transform ??= owner.GetComponent<Transform2D>();
        transform ??= owner.CreateComponent<Transform2D>();

        lock (pSync)
            pData[id].Transform = new WeakReference<Transform2D>(transform);
        return transform;
    }

    public SpriteEffect? GetEffect(int id)
    {
        SpriteEffect? effect;
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            effect = pData[id].Effect;
        }

        return effect;
    }

    public SpriteInstanceType GetInstanceType(int id)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            return pData[id].Type;
        }
    }    
    
    public SpriteBatchItem[] GetItems(int id)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            return pData[id].Items;
        }   
    }

    public void SetEffect(int id, SpriteEffect? effect)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            pData[id].Dirty |= pData[id].Effect != effect;
            pData[id].Effect = effect;
        }
    }

    public void SetInstanceType(int id, SpriteInstanceType type)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            if (pData[id].Type != type)
            {
                if(pData[id].Batch is not null)
                    pLogger.Warning($"Change Instance Type can be expensive. Avoid this operation as possible");
                pData[id].Dirty = true;
                DisposableQueue.Enqueue(pData[id].Batch);
                pData[id].Batch = null;
            }

            pData[id].Type = type;
        }
    }

    public void SetItems(int id, SpriteBatchItem[] items)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            if (pData[id].Type == SpriteInstanceType.Static && pData[id].Batch is not null)
                pLogger.Warning("Update Instance Items is too expensive. Avoid as possible");
            pData[id].Dirty = true;
            pData[id].Items = items;
        }
    }
    protected override void OnDispose()
    {
        mExecutionPipeline
            .AddEvent(SpriteSystemEvents.EndUpdate, OnEndUpdate)
            .AddEvent(SpriteSystemEvents.Render, OnRender);
        lock (pSync)
        {
            if (pAvailableIdx.Count == pData.Length)
                return;
            foreach (var data in pData)
            {
                if(data.Ref is null)
                    continue;
                DisposableQueue.Enqueue(data.Batch);
            }

            pData = [];
            pAvailableIdx.Clear();
        }
    }

    private void OnEndUpdate(IExecutionPipeline _)
    {
        lock (pSync)
        {
            if (pAvailableIdx.Count == pData.Length)
                return;

            var totalItems = pData.Length - pAvailableIdx.Count;
            var processItems = 0;
            var nextIdx = 0;

            while (processItems < totalItems && nextIdx < pData.Length)
            {
                var idx = nextIdx;
                ++nextIdx;
                if(pData[idx].Ref is null)
                    continue;
                VerifyItem(idx);
                ++processItems;
            }
        }
    }

    private void VerifyItem(int id)
    {
        var data = pData[id];
        if (data.Ref?.Owner is null)
            return;

        var transform = GetTransform(id);
        transform.GetSnapshot(out var currSnapshot);

        var dirty = data.Dirty;
        dirty |= !currSnapshot.Equals(pData[id].LastTransformSnapshot);
        dirty |= pData[id].Enabled != data.Ref.Enabled;
        dirty |= pData[id].Batch is null;

        pData[id].Enabled = data.Ref.Enabled;
        pData[id].LastTransformSnapshot = currSnapshot;
        pData[id].Dirty = false;
        
        if(dirty)
            pComponents2Update.Enqueue(id);
    }

    private void OnRender(IExecutionPipeline _)
    {
        while(pComponents2Update.TryDequeue(out var componentId))
            BuildRenderItem(componentId);
    }

    private void BuildRenderItem(int componentId)
    {
        SpriteInstanceData data;
        Transform2DSnapshot transform2DSnapshot;
        SpriteEffect? effect;
        SpriteInstanceBatch? batch;
        bool enabled;

        lock (pSync)
        {
            if (pData[componentId].Ref is null)
                return;
            enabled = pData[componentId].Ref?.Enabled ?? false;
            data = pData[componentId];
            batch = pData[componentId].Batch;
            effect = pData[componentId].Effect;
            transform2DSnapshot = pData[componentId].LastTransformSnapshot;
        }

        effect ??= pSpriteBatch.DefaultEffect;

        batch ??= CreateInstanceBatch(data.Type);
        batch.Update(new SpriteInstanceBatchItemDesc()
        {
            Enabled = enabled,
            Effect = effect,
            Transform = transform2DSnapshot.WorldTransformMatrix,
            Items = data.Items
        });
    }

    private SpriteInstanceBatch CreateInstanceBatch(SpriteInstanceType type)
    {
        SpriteInstanceBatch batch;
        switch (type)
        {
            case SpriteInstanceType.Default:
                batch = pSpriteBatch.CreateDefaultSprite();
                break;
            case SpriteInstanceType.Dynamic:
                batch = pSpriteBatch.CreateDynamicSprite();
                break;
            case SpriteInstanceType.Static:
                batch = pSpriteBatch.CreateStaticSprite();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return batch;
    }

    protected override void ValidateId(int id)
    {
        base.ValidateId(id);
        lock (pSync)
        {
            if (pData[id].Ref is null)
                throw new InvalidComponentId(id, typeof(SpriteInstanceSystem));
        }
    }
}    