using System.Drawing;
using System.Numerics;
using REngine.RHI;
using REngine.RPI.Structs;

namespace REngine.RPI;

public abstract class BaseSpriteBatch(int id) : IDisposable
{
    private bool pIsLocked = false;

    public int Id
    {
        get
        {
#if DEBUG
            ValidateDispose();
#endif
            return id;
        }
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        OnDispose();
    }

    public void Lock()
    {
#if DEBUG
        ValidateDispose();
#endif
        Monitor.Enter(GetObjectSync());
        pIsLocked = true;
    }

    public void Unlock()
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        Monitor.Exit(GetObjectSync());
        pIsLocked = false;
    }

    protected void ValidateLock()
    {
        if (!pIsLocked)
            throw new Exception("Batch must lock first.");
    }

    protected void ValidateDispose()
    {
        if (IsDisposed)
            throw new ObjectDisposedException("Batch is already disposed");
    }

    protected abstract object GetObjectSync();
    protected abstract void OnDispose();
}

public sealed class SpriteBatch(int id, SpriteBatchSystem batchSystem) : BaseSpriteBatch(id)
{
    public bool Enabled
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.IsEnabled(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetEnabled(id, value);
        }
    }

    public Vector3 Position
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetPosition(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetPosition(id, value);
        }
    }

    public Vector2 Anchor
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetAnchor(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetAnchor(id, value);
        }
    }

    public Vector2 Offset
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetAnchor(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetOffset(id, value);
        }
    }

    public Vector2 Size
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetSize(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetSize(id, value);
        }
    }

    public float Angle
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetAngle(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetAngle(id, value);
        }
    }

    public Color Color
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetColor(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetColor(id, value);
        }
    }

    public ITexture? Texture
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetTexture(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetTexture(id, value);
        }
    }

    public IShaderResourceBinding? ShaderResourceBinding
    {
        get
        {
#if DEBUG
            ValidateDispose();
#endif
            return batchSystem.GetShaderResourceBinding(id);
        }
    }

    public IPipelineState? PipelineState
    {
        get
        {
#if DEBUG
            ValidateDispose();
#endif
            return batchSystem.GetPipelineState(id);
        }
    }

    public SpriteEffect? Effect
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetEffect(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetEffect(id, value);
        }
    }

    public bool IsDirty
    {
        get
        {
#if DEBUG
            ValidateDispose();
#endif
            return batchSystem.IsDirty(id);
        }
    }

    protected override object GetObjectSync() => batchSystem.GetObjectSync(id);

    protected override void OnDispose()
    {
        batchSystem.Destroy(id);
    }
}

public sealed class SpriteInstanceBatch(int id, SpriteInstancedBatchSystem batchSystem) : BaseSpriteBatch(id)
{
    public bool Enabled
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.IsEnabled(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetEnabled(id, value);
        }
    }

    public Color Color
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetColor(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetColor(id, value);
        }
    }

    public ITexture? Texture
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetTexture(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            batchSystem.SetTexture(id, value);
        }
    }

    public int InstanceCount
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetInstanceCount(id);
        }
    }

    public IShaderResourceBinding? ShaderResourceBinding
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetShaderResourceBinding(id);
        }
    }

    public IPipelineState? PipelineState
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetPipelineState(id);
        }
    }

    public bool IsDirty
    {
        get
        {
#if DEBUG
            ValidateDispose();
#endif
            return batchSystem.IsDirty(id);
        }
    }

    public void ResizeInstances(uint numInstances, bool dynamic = false)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        batchSystem.ResizeInstances(id, numInstances, dynamic);
    }

    public Vector2 GetInstancePosition(uint instanceIdx)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        batchSystem.GetElement(id, instanceIdx, out var element);
        return element.Position;
    }

    public Vector2 GetInstanceScale(uint instanceIdx)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        batchSystem.GetElement(id, instanceIdx, out var element);
        return element.Scale;
    }

    public float GetInstanceAngle(uint instanceIdx)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        batchSystem.GetElement(id, instanceIdx, out var element);
        return element.Angle;
    }

    public Vector2 GetInstanceAnchor(uint instanceIdx)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        batchSystem.GetElement(id, instanceIdx, out var element);
        return element.Anchor;
    }

    public float GetInstanceDepth(uint instanceIdx)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        batchSystem.GetElement(id, instanceIdx, out var element);
        return element.Depth;
    }

    public SpriteInstanceBatch SetInstancePosition(uint instanceIdx, Vector2 position)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        batchSystem.GetElement(id, instanceIdx, out var element);
        element.Position = position;
        batchSystem.SetElement(id, instanceIdx, ref element);
        return this;
    }

    public SpriteInstanceBatch SetInstanceScale(uint instanceIdx, Vector2 scale)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        batchSystem.GetElement(id, instanceIdx, out var element);
        element.Scale = scale;
        batchSystem.SetElement(id, instanceIdx, ref element);
        return this;
    }

    public SpriteInstanceBatch SetInstanceAngle(uint instanceIdx, float angle)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        batchSystem.GetElement(id, instanceIdx, out var element);
        element.Angle = angle;
        batchSystem.SetElement(id, instanceIdx, ref element);
        return this;
    }

    public SpriteInstanceBatch SetInstanceAnchor(uint instanceIdx, Vector2 anchor)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        batchSystem.GetElement(id, instanceIdx, out var element);
        element.Anchor = anchor;
        batchSystem.SetElement(id, instanceIdx, ref element);
        return this;
    }

    public SpriteInstanceBatch SetInstanceDepth(uint instanceIdx, float depth)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        batchSystem.GetElement(id, instanceIdx, out var element);
        element.Depth = depth;
        batchSystem.SetElement(id, instanceIdx, ref element);
        return this;
    }

    protected override object GetObjectSync() => batchSystem.GetSyncObject(id);

    protected override void OnDispose()
    {
        batchSystem.DestroyBatch(id);
    }
}