using System.Drawing;
using System.Numerics;
using REngine.Core.WorldManagement;
using REngine.RHI;
using REngine.RPI.Structs;

namespace REngine.RPI;

public sealed class SpriteBatchSystem(
    RenderSettings renderSettings) : BaseSystem<SpriteBatchItem>((int)renderSettings.SpriteBatchInitialSize)
{
    private readonly object pSync = new();

    public SpriteBatch CreateBatch()
    {
        SpriteBatch batch;
        lock (pSync)
        {
            var id = Acquire();
            batch = new SpriteBatch(id, this);
            pData[id] = new SpriteBatchItem
            {
                RefBatch = batch
            };
        }

        return batch;
    }

    public SpriteBatchSystem Destroy(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            var data = pData[id];
            if (data.RefBatch is null)
                return this;
            data.RefBatch.Dispose();
            data.RefBatch = null;
            pAvailableIdx.Enqueue(id);
        }

        return this;
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
            if (data.RefBatch is null)
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

    public ITexture? GetTexture(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Texture;
        }
    }

    public IShaderResourceBinding? GetShaderResourceBinding(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].ShaderResourceBinding;
        }
    }

    public IPipelineState? GetPipelineState(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].PipelineState;
        }
    }

    public SpriteEffect? GetEffect(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Effect;
        }
    }

    public bool IsDirty(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Dirty;
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

    public void SetTexture(int id, ITexture texture)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Texture = texture;
            pData[id].Dirty = true;
        }
    }

    public void SetShaderResourceBinding(int id, IShaderResourceBinding? srb)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].ShaderResourceBinding = srb;
            pData[id].Dirty = true;
        }
    }

    public void SetPipelineState(int id, IPipelineState? pipelineState)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].PipelineState = pipelineState;
            pData[id].Dirty = true;
        }
    }

    public void SetEffect(int id, SpriteEffect? effect)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Effect = effect;
            pData[id].Dirty = true;
        }
    }

    public void RemoveDirtyState(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Dirty = false;
        }
    }

    public SpriteBatchSystem ForEach(Action<SpriteBatch> callback)
    {
        lock (pSync)
        {
            if (pAvailableIdx.Count == pData.Length)
                return this;

            foreach (var data in pData)
            {
                if (data.RefBatch is null)
                    continue;
                callback(data.RefBatch);
            }
        }

        return this;
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
            if (pData[id].RefBatch is null)
                throw new Exception("Invalid Sprite Batch. it seems this batch is already destroyed");
        }
    }
}