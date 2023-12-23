using System.Drawing;
using System.Numerics;
using REngine.Core.Mathematics;
using REngine.Core.WorldManagement;
using REngine.RHI;
using REngine.RPI.Events;
using REngine.RPI.Structs;
using REngine.RPI.Utils;

namespace REngine.RPI;

public sealed class SpriteSystem(
    RenderSettings renderSettings,
    BatchSystem batchSystem,
    IServiceProvider provider,
    IBufferManager bufferManager)
    : BaseSystem<SpriteBatchItem>((int)renderSettings.SpriteBatchInitialSize)
{
    public const string BatchGroupName = nameof(Sprite);
    private struct GpuData()
    {
        public Matrix4x4 Transform = Matrix4x4.Identity;
        public Vector4 Color = Vector4.One;
    }
    private class InternalBatch(SpriteSystem system, int id, IBuffer constantBuffer) : QuadBatch
    {
        public override void Render(ICommandBuffer command)
        {
            Sprite? sprite;
            lock (system.pSync)
                sprite = system.pData[id].RefSprite;

            if (sprite is null)
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
            var gpuData = new GpuData
            {
                Transform = MatrixUtils.GetSpriteTransform(
                    sprite.Position,
                    sprite.Anchor,
                    sprite.Angle,
                    sprite.Size
                ),
                Color = sprite.Color.ToVector4()
            };

            sprite.Unlock();
            effect.UpdateBuffers();
            
            var mapped = command.Map<GpuData>(constantBuffer, MapType.Write, MapFlags.Discard);
            mapped[0] = gpuData;
            command.Unmap(constantBuffer, MapType.Write);
            base.Render(command);
        }
    }
    
    private readonly object pSync = new();
    private readonly BatchGroup pBatchGroup = batchSystem.GetGroup(BatchGroupName);
    private readonly SpriteEffect pDefaultEffect = SpriteEffect.Build(provider);

    public Sprite Create(SpriteEffect? effect)
    {
        Sprite sprite;
        lock (pSync)
        {
            var id = Acquire();
            var batch = new InternalBatch(this, id, bufferManager.GetBuffer(BufferGroupType.Object));
            effect ??= pDefaultEffect;
            
            sprite = new Sprite(id, this);
            pData[id] = new SpriteBatchItem(effect)
            {
                RefSprite = sprite,
                BatchIndex = pBatchGroup.AddBatch(batch)
            };
        }

        return sprite;
    }

    public void Destroy(int id)
    {
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
            pBatchGroup.RemoveBatch(data.BatchIndex);
            pAvailableIdx.Enqueue(id);
        }
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

    public Vector2 GetOffset(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Offset;
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

    public void SetOffset(int id, Vector2 offset)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Offset = offset;
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

    public void ForEach(Action<Sprite> callback)
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