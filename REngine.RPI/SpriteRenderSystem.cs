using System.Drawing;
using System.Numerics;
using REngine.Core;
using REngine.Core.Mathematics;
using REngine.Core.WorldManagement;
using REngine.RHI;
using REngine.RPI.Events;
using REngine.RPI.Structs;
using REngine.RPI.Utils;

namespace REngine.RPI;

public sealed class SpriteRenderSystem(
    RenderSettings renderSettings,
    BatchSystem batchSystem,
    IServiceProvider provider,
    IBufferManager bufferManager,
    RenderState renderState)
    : BaseSystem<SpriteBatchItem>((int)renderSettings.SpriteBatchInitialSize)
{
    public const string BatchGroupName = nameof(SpriteRenderItem);
    private struct GpuData()
    {
        public Matrix4x4 Transform = Matrix4x4.Identity;
        public Vector4 Color = Vector4.One;
    }
    private class InternalBatch(
        SpriteRenderSystem renderSystem, 
        int id, 
        IBuffer constantBuffer,
        RenderState renderState) : QuadBatch
    {
        public override int GetSortIndex()
        {
            lock(renderSystem.pSync)
                return (int)Mathf.FloatToInt(renderSystem.pData[id].Position.Z);
        }

        public override void Render(BatchRenderInfo batchRenderInfo)
        {
            var command = batchRenderInfo.CommandBuffer;
            SpriteRenderItem? sprite;
            lock (renderSystem.pSync)
                sprite = renderSystem.pData[id].RefSprite;

            if (sprite is null || sprite.IsDisposed)
                return;

            sprite.Lock();

            // Skip rendering if sprite is disabled
            if (!sprite.Enabled)
            {
                sprite.Unlock();
                return;
            }
            
            PipelineState = sprite.Effect.OnBuildPipeline();
            ShaderResourceBinding = sprite.Effect.OnGetShaderResourceBinding();

            var effect = sprite.Effect;
            var pos = sprite.Position;
            pos.Z = pos.Z / ushort.MaxValue;
            var gpuData = new GpuData
            {
                Transform = MatrixUtils.GetSpriteTransform(
                    pos,
                    sprite.Anchor,
                    sprite.Angle,
                    sprite.Size
                ) * renderState.FrameData.ScreenProjection,
                Color = sprite.Color.ToVector4()
            };

            sprite.Unlock();
            effect.UpdateBuffers();
            
            var mapped = command.Map<GpuData>(constantBuffer, MapType.Write, MapFlags.Discard);
            mapped[0] = gpuData;
            command.Unmap(constantBuffer, MapType.Write);
            base.Render(batchRenderInfo);
        }
    }
    
    private readonly object pSync = new();
    private readonly BatchGroup pBatchGroup = batchSystem.GetGroup(BatchGroupName);
    public SpriteEffect DefaultEffect { get; } = SpriteEffect.Build(provider);

    public SpriteRenderItem Create(SpriteEffect? effect = null)
    {
        SpriteRenderItem spriteRenderItem;
        pBatchGroup.Lock();
        lock (pSync)
        {
            var id = Acquire();
            var batch = new InternalBatch(
                this, 
                id, 
                bufferManager.GetBuffer(BufferGroupType.Object),
                renderState);
            effect ??= DefaultEffect;
            
            spriteRenderItem = new SpriteRenderItem(id, this);
            pData[id] = new SpriteBatchItem(effect)
            {
                RefSprite = spriteRenderItem,
                Batch = pBatchGroup.AddBatch(batch)
            };
        }
        pBatchGroup.Unlock();

        return spriteRenderItem;
    }

    public void Destroy(int id)
    {
        pBatchGroup.Lock();
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            var data = pData[id];
            if (data.RefSprite is null)
                return;
            
            data.RefSprite.Dispose();
            data.RefSprite = null;
            pBatchGroup.RemoveBatch(data.Batch);
            pAvailableIdx.Enqueue(id);
        }
        pBatchGroup.Unlock();
    }

    public void DestroyBatches()
    {
        lock (pSync)
        {
            if (pAvailableIdx.Count == pData.Length)
                return;
            foreach (var data in pData)
                data.RefSprite?.Dispose();
            
            pAvailableIdx.Clear();
            pData = [];
        }
    }
    public object GetObjectSync(int id)
    {
        object obj;
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            var data = pData[id];
            if (data.RefSprite is null)
                return this;
            obj = pData[id].Sync;
        }

        return obj;
    }

    public bool IsEnabled(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Enabled;
        }
    }

    public Vector3 GetPosition(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Position;
        }
    }

    public Vector2 GetAnchor(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Anchor;
        }
    }

    public Vector2 GetSize(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Size;
        }
    }

    public float GetAngle(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Angle;
        }
    }

    public Color GetColor(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Color;
        }
    }

    public SpriteEffect GetEffect(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Effect;
        }
    }
    
    public void SetEnabled(int id, bool enabled)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Enabled = enabled;
        }
    }

    public void SetPosition(int id, Vector3 position)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Position = position;
        }
    }

    public void SetAnchor(int id, Vector2 anchor)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Anchor = anchor;
        }
    }

    public void SetSize(int id, Vector2 size)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Size = size;
        }
    }

    public void SetAngle(int id, float angle)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Angle = angle;
        }
    }

    public void SetColor(int id, Color color)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Color = color;
        }
    }

    public void SetEffect(int id, SpriteEffect effect)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Effect = effect;
        }
    }

    public void ForEach(Action<SpriteRenderItem> callback)
    {
        lock (pSync)
        {
            if (pAvailableIdx.Count == pData.Length)
                return;

            foreach (var data in pData)
            {
                if (data.RefSprite is null)
                    continue;
                callback(data.RefSprite);
            }
        }
    }

    protected override int GetExpansionSize() =>
        (int)Math.Round(
            Math.Max(
                (float)renderSettings.SpriteBatchInitialSize * renderSettings.SpriteBatchExpansionRatio,
                renderSettings.SpriteBatchInitialSize
            )
        );

    protected override void ValidateId(int id)
    {
        base.ValidateId(id);
        lock (pSync)
        {
            if (pData[id].RefSprite is null)
                throw new Exception("Invalid Sprite Batch. it seems this batch is already destroyed");
        }
    }
    
    
}