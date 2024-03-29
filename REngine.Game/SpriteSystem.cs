using System.Buffers;
using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using REngine.Core;
using REngine.Core.Events;
using REngine.Core.Exceptions;
using REngine.Core.IO;
using REngine.Core.Threading;
using REngine.Core.WorldManagement;
using REngine.Game.Components;
using REngine.RPI;
using REngine.RPI.Batches;
using REngine.RPI.Effects;
using REngine.RPI.Structs;

namespace REngine.Game;

[Flags]
public enum SpriteMouseState
{
    None = 0,
    Enter,
    Exit
}

public struct SpriteData()
{
    // Component Data vars
    public bool Enabled;
    public bool Dirty;
    public Transform2DSnapshot LastTransformSnapshot;
    public Vector2 Anchor;

    public Color Color = Color.White;

    // Mouse vars
    public SpriteMouseState MouseState;

    public bool Clicked;

    // Dependencies vars
    public SpriteEffect? Effect;
    public SpriteBatch? Batch;
    public SpriteComponent? Ref;

    public WeakReference<Transform2D> Transform = new(null);

    // Event vars
    public readonly EventQueue ClickEvent = new();
    public readonly EventQueue MouseEnterEvent = new();
    public readonly EventQueue MouseExitEvent = new();
}

public sealed class SpriteSystem : BehaviorSystem<SpriteData>
{
    private readonly object pSync = new();
    private readonly ConcurrentQueue<int> pComponents2Update = new();
    private readonly ISpriteBatch pSpriteBatch;
    private readonly IInput pInput;

    public SpriteSystem(
        IExecutionPipeline executionPipeline,
        EngineEvents engineEvents,
        ISpriteBatch spriteBatch,
        IInput input
    ) : base(executionPipeline, engineEvents, BehaviorSystemEventFlags.None)
    {
        pSpriteBatch = spriteBatch;
        pInput = input;
    }

    protected override void OnEngineStart()
    {
        mExecutionPipeline
            .AddEvent(SpriteSystemEvents.EndUpdate, OnEndUpdate)
            .AddEvent(SpriteSystemEvents.Render, OnRender);
    }

    public SpriteComponent Create()
    {
        SpriteComponent component;
        lock (pSync)
        {
            var id = Acquire();
            component = new SpriteComponent(id, this);
            pData[id] = new SpriteData()
            {
                Ref = component,
                Dirty = true
            };
        }

        return component;
    }

    public void Destroy(int id)
    {
        lock (pSync)
        {
            if (pData[id].Ref is null || pAvailableIdx.Count == pData.Length)
                return;
            pData[id].Ref = null;
            pData[id].MouseEnterEvent.ClearAllListeners();
            pData[id].MouseExitEvent.ClearAllListeners();
            pData[id].ClickEvent.ClearAllListeners();
            // RPI Objects must dispose on Main Thread
            DisposableQueue.Enqueue(pData[id].Batch);
            pAvailableIdx.Enqueue(id);
        }
    }
    public void Destroy(SpriteComponent component)
    {
        lock (pSync)
        {
            if (component.Id >= pData.Length || pAvailableIdx.Count == pData.Length)
                return;
            var targetComponent = pData[component.Id].Ref;
            if (targetComponent is null || targetComponent != component)
                return;
        }
        
        Destroy(component.Id);
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

    public Vector2 GetAnchor(int id)
    {
        Vector2 anchor;
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            anchor = pData[id].Anchor;
        }

        return anchor;
    }

    public Color GetColor(int id)
    {
        Color color;
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            color = pData[id].Color;
        }

        return color;
    }

    public EventQueue GetClickEvent(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].ClickEvent;
        }
    }

    public EventQueue GetMouseEnterEvent(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].MouseEnterEvent;
        }
    }

    public EventQueue GetMouseExitEvent(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].MouseExitEvent;
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

    public void SetAnchor(int id, Vector2 anchor)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            pData[id].Dirty |= pData[id].Anchor != anchor;
            pData[id].Anchor = anchor;
        }
    }

    public void SetColor(int id, Color color)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            pData[id].Dirty |= pData[id].Color != color;
            pData[id].Color = color;
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
                if (pData[idx].Ref is null)
                    continue;
                ComputeMouseInput(idx);
                VerifyItem(idx);
                ++processItems;
            }
        }
    }

    private void ComputeMouseInput(int id)
    {
        var transform = GetTransform(id);
        var pos = transform.WorldPosition;
        var scale = transform.Scale;
        var msPos = pInput.MousePosition;
        var msClick = pInput.GetMouseDown(MouseKey.Left);
        var clicked = pData[id].Clicked;
        var bounds = new RectangleF(pos.X, pos.Y, scale.X, scale.Y);

        var msState = pData[id].MouseState;
        var clickEvt = pData[id].ClickEvent;
        var enterEvt = pData[id].MouseEnterEvent;
        var exitEvt = pData[id].MouseExitEvent;

        var isInside = bounds.Contains(msPos.X, msPos.Y);
        switch (msState)
        {
            case SpriteMouseState.None when isInside:
                msState = SpriteMouseState.Enter;
                enterEvt.Invoke(pData[id].Ref);
                break;
            case SpriteMouseState.Enter when !isInside:
                msState = SpriteMouseState.Exit;
                exitEvt.Invoke(pData[id].Ref);
                break;
            case SpriteMouseState.Exit:
                msState = SpriteMouseState.None;
                break;
        }

        if (msClick && !clicked && isInside)
        {
            pData[id].Clicked = true;
            clickEvt.Invoke(pData[id].Ref);
        }

        pData[id].MouseState = msState;
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

        pData[id].Enabled = data.Ref.Enabled;
        pData[id].LastTransformSnapshot = currSnapshot;
        pData[id].Dirty = false;

        if (dirty)
            pComponents2Update.Enqueue(id);
    }

    private void OnRender(IExecutionPipeline _)
    {
        while (pComponents2Update.TryDequeue(out var componentId))
            BuildRenderItem(componentId);
    }

    private void BuildRenderItem(int componentId)
    {
        SpriteData data;
        Transform2DSnapshot transformSnapshot;
        SpriteEffect? effect;
        SpriteBatch? spriteBatch;
        bool enabled;

        lock (pSync)
        {
            if (pData[componentId].Ref is null)
                return;
            enabled = pData[componentId].Ref?.Enabled ?? false;
            data = pData[componentId];
            spriteBatch = pData[componentId].Batch;
            effect = pData[componentId].Effect;
            transformSnapshot = pData[componentId].LastTransformSnapshot;
        }

        effect ??= pSpriteBatch.DefaultEffect;

        spriteBatch ??= pSpriteBatch.CreateSprite();
        spriteBatch.Update(new SpriteBatchItemDesc()
        {
            Enabled = enabled,
            Effect = effect,
            Item = new SpriteBatchItem()
            {
                Anchor = data.Anchor,
                Color = data.Color,
                Position = new Vector3(transformSnapshot.WorldPosition, transformSnapshot.WorldRotation),
                Rotation = transformSnapshot.Rotation,
                Scale = transformSnapshot.Scale,
            }
        });

        lock (pSync)
        {
            // if component goes to destroy, then add sprite to disposable queue
            if (pData[componentId].Ref is null)
            {
                DisposableQueue.Enqueue(spriteBatch);
                return;
            }

            pData[componentId].Batch = spriteBatch;
        }
    }

    protected override void OnDispose()
    {
        mExecutionPipeline
            .RemoveEvent(SpriteSystemEvents.EndUpdate, OnEndUpdate)
            .RemoveEvent(SpriteSystemEvents.Render, OnRender);
        lock (pSync)
        {
            if (pAvailableIdx.Count == pData.Length)
                return;
            foreach (var data in pData)
            {
                if (data.Ref is null)
                    continue;
                DisposableQueue.Enqueue(data.Batch);
            }

            pData = [];
            pAvailableIdx.Clear();
        }
    }

    protected override int GetExpansionSize()
    {
        return 1;
    }

    protected override void ValidateId(int id)
    {
        base.ValidateId(id);
        lock (pSync)
        {
            if (pData[id].Ref is null)
                throw new InvalidComponentId(id, typeof(SpriteSystem));
        }
    }
}