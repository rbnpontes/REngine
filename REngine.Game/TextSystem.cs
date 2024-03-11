using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.Core.Threading;
using REngine.Core.WorldManagement;
using REngine.Game.Components;
using REngine.RHI;
using REngine.RPI;
using REngine.RPI.Batches;
using REngine.RPI.Effects;

namespace REngine.Game;

public struct TextData()
{
    public bool Enabled;
    public bool IsDynamic = true;
    public string Text = string.Empty;
    public float VerticalSpacing;
    public float HorizontalSpacing;
    public Color Color = Color.White;
    public string FontName = string.Empty;
    public ulong FontHash;

    public RectangleF Bounds = new();
    public Transform2DSnapshot LastSnapshot;
    public bool Dirty = true;

    public ulong TextHash;
    public ulong LastTextHash;
    
    public TextEffect? Effect;
    public TextBatch? Batch;
    public ITexture? FontAtlas;

    public WeakReference<Transform2D> Transform = new(null);

    public TextComponent? Ref;
}

public sealed class TextSystem : BehaviorSystem<TextData>, IDisposable
{
    private const float FontRatio = 1 / 16f;
    private readonly object pSync = new();
    private readonly IFontSystem pFontSystem;
    private readonly IExecutionPipeline pExecutionPipeline;
    private readonly EngineEvents pEngineEvents;
    private readonly ILogger<TextSystem> pLogger;
    private readonly ISpriteBatch pSpriteBatch;

    private bool pDisposed;

    public TextSystem(
        IExecutionPipeline executionPipeline,
        EngineEvents engineEvents,
        ILoggerFactory loggerFactory,
        IFontSystem fontSystem,
        ISpriteBatch spriteBatch) : base(executionPipeline, engineEvents, BehaviorSystemEventFlags.None)
    {
        pLogger = loggerFactory.Build<TextSystem>();
        pExecutionPipeline = executionPipeline;
        pEngineEvents = engineEvents;
        pFontSystem = fontSystem;
        pSpriteBatch = spriteBatch;
        
        engineEvents.OnStart.Once(OnEngineStart);
        engineEvents.OnBeforeStop.Once(OnEngineStop);
    }

    private async Task OnEngineStop(object sender)
    {
        Dispose();
    }
    private async Task OnEngineStart(object sender)
    {
        pExecutionPipeline
            .AddEvent(SpriteSystemEvents.EndUpdate, OnEndUpdate);
    }
    public void Dispose()
    {
        if (pDisposed)
            return;

        DestroyAll();
        lock(pSync)
            pAvailableIdx.Clear();
        pDisposed = true;
    }
    public void DestroyAll()
    {
        lock (pSync)
        {
            if (pAvailableIdx.Count == pData.Length)
                return;
            for (var i = 0; i < pData.Length; ++i)
            {
                if(pData[i].Ref is null)
                    continue;
                pData[i].Ref = null;
                DisposableQueue.Enqueue(pData[i].Batch);
                pData[i].Batch = null;
                pAvailableIdx.Enqueue(i);
            }
        }
    }

    public void Destroy(TextComponent component)
    {
        lock (pSync)
        {
            var id = component.Id;
            if (pData.Length == pAvailableIdx.Count || id >= pData.Length)
                return;

            pData[id].Ref = null;
            DisposableQueue.Enqueue(pData[id].Batch);
            pAvailableIdx.Enqueue(id);
        }
    }
    public int Create(TextComponent component)
    {
        lock (pSync)
        {
            var id = Acquire();
            pData[id] = new TextData()
            {
                Ref = component
            };

            return id;
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
    public TextEffect? GetEffect(int id)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            return pData[id].Effect;
        }
    }
    public bool IsDynamic(int id)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            return pData[id].IsDynamic;
        }
    }
    public string GetText(int id)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            return pData[id].Text;
        }
    }
    public float GetVerticalSpacing(int id)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            return pData[id].VerticalSpacing;
        }
    }
    public float GetHorizontalSpacing(int id)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            return pData[id].HorizontalSpacing;
        }
    }
    public Color GetColor(int id)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            return pData[id].Color;
        }
    }
    public string GetFontName(int id)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            return pData[id].FontName;
        }
    }
    public RectangleF GetBounds(int id)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            return pData[id].Bounds;
        }
    }
    public void SetEffect(int id, TextEffect? effect)
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
    public void SetIsDynamic(int id, bool value)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            if (pData[id].IsDynamic != value && pData[id].Batch is not null)
            {
                pLogger.Warning("Change Text from Dynamic to non dynamic at runtime can cause slow down. Try to avoid as possible");
                DisposableQueue.Enqueue(pData[id].Batch);
                pData[id].Batch = null;
            }

            pData[id].Dirty |= pData[id].IsDynamic != value;
            pData[id].IsDynamic = value;
        }
    }
    public void SetText(int id, string value)
    {
        value = string.IsNullOrEmpty(value) ? string.Empty : value;
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            var hash = Hash.Digest(value);
            pData[id].Dirty |= pData[id].TextHash != hash;
            pData[id].TextHash = hash;
            pData[id].Text = value;
        }
    }
    public void SetVerticalSpacing(int id, float value)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            if (Math.Abs(value - pData[id].VerticalSpacing) > float.Epsilon)
            {
                pData[id].Dirty = true;
                pData[id].LastTextHash = 0; // Force text recalculation
            }
            pData[id].VerticalSpacing = value;
        }
    }
    public void SetHorizontalSpacing(int id, float value)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            if (Math.Abs(value - pData[id].HorizontalSpacing) > float.Epsilon)
            {
                pData[id].Dirty = true;
                pData[id].LastTextHash = 0; // Force text recalculation
            }
            pData[id].HorizontalSpacing = value;
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
    public void SetFontName(int id, string fontName)
    {
        lock (pSync)
        {
#if RENGINE_VALIDATIONS
            ValidateId(id);
#endif
            var fontHash = Hash.Digest(fontName);
            if (fontHash != pData[id].FontHash)
            {
                pData[id].Dirty = true;
                pData[id].FontAtlas = null;
            }
            pData[id].FontName = fontName;
            pData[id].FontHash = fontHash;
        }
    }
    public void SetFont(int id, Font font)
    {
        SetFontName(id, font.Name);
        pFontSystem.SetFont(font);
    }
    private void OnEndUpdate(IExecutionPipeline _)
    {
        lock (pSync)
        {
            if(pAvailableIdx.Count == pData.Length)
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
                ValidateTransform(idx);
                ValidateBatch(idx);
                if(pData[idx].Dirty)
                    UpdateItem(idx);
                ++processItems;
            }
        }
    }

    private void ValidateTransform(int id)
    {
        var transform = GetTransform(id);
        transform.GetSnapshot(out var currSnapshot);
        pData[id].Dirty |= !pData[id].LastSnapshot.Equals(currSnapshot);
    }

    private void ValidateBatch(int id)
    {
        pData[id].Dirty |= pData[id].FontAtlas is null || pData[id].Batch is null;
        if (pData[id].FontAtlas is null || !pData[id].FontAtlas.IsDisposed)
            return;
        pData[id].FontAtlas = null;
        pData[id].Dirty = true;
    }
    private void UpdateItem(int id)
    {
        var data = pData[id];
        if (data.Ref?.Owner is null || data.FontHash == 0ul)
            return;

        var font = pFontSystem.GetFont(data.FontHash);
        var fontAtlas = pData[id].FontAtlas ?? pFontSystem.GetFontAtlas(data.FontHash);
        
        // Try to pick font atlas before continue
        // If atlas is not ready, then skip verification
        // until font atlas is available.
        if (font is null || fontAtlas is null)
        {
            pData[id].FontAtlas = fontAtlas;
            return;
        }

        // If atlas has been destroyed, then skip verification
        if (fontAtlas.IsDisposed)
        {
            pData[id].FontAtlas = null;
            return;
        }

        pData[id].FontAtlas = fontAtlas;
        
        var batch = pData[id].Batch ?? pSpriteBatch.CreateTextBatch(new TextCreateInfo(font, fontAtlas, data.IsDynamic));
        var transform = GetTransform(id);
        transform.GetSnapshot(out var currSnapshot);

        if (pData[id].Batch != batch || data.LastTextHash != data.TextHash)
        {
            batch.Update(new TextBatchDesc()
            {
                Color = data.Color,
                Text = data.Text,
                Effect = data.Effect,
                Transform = currSnapshot.WorldTransformMatrix,
                Enabled = data.Ref.Enabled,
                HorizontalSpacing = data.HorizontalSpacing,
                VerticalSpacing = data.VerticalSpacing,
                ZIndex = currSnapshot.ZIndex
            });
        } 
        else if (data.Enabled != data.Ref.Enabled || !currSnapshot.Equals(pData[id].LastSnapshot) || data.Dirty)
        {
            batch.Update(new TextBatchBasicUpdateDesc()
            {
                Enabled = data.Enabled,
                Color = data.Color,
                Effect = data.Effect,
                Transform = currSnapshot.WorldTransformMatrix,
                ZIndex = currSnapshot.ZIndex
            });
        }

        var bounds = batch.Bounds;
        
        bounds.Location = currSnapshot.WorldPosition.ToPoint();
        bounds.Size = (bounds.Size.ToVector2() * currSnapshot.Scale).ToSize();
        pData[id].Dirty = false;
        pData[id].Enabled = data.Ref.Enabled;
        pData[id].Batch = batch;
        pData[id].LastTextHash = pData[id].TextHash;
        pData[id].LastSnapshot = currSnapshot;
        pData[id].Bounds = bounds;
    }
}
