using System.Drawing;
using System.Numerics;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI.Structs;

namespace REngine.RPI;

public abstract class BaseSprite(int id) : IDisposable
{
    private bool pIsLocked;
#if DEBUG
    private string pThreadLockName = string.Empty;
    private ulong pThreadId;
#endif

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
        lock (GetObjectSync())
        {
#if DEBUG
            pThreadLockName = Thread.CurrentThread.Name ?? "Unknown Thread";
            pThreadId = Hash.Digest(pThreadLockName);
#endif
            IsDisposed = true;
            OnDispose();
        }
        
        GC.SuppressFinalize(this);
    }

    public void Lock()
    {
        Core.Threading.Monitor.Enter(GetObjectSync());
#if DEBUG
        pThreadLockName = Thread.CurrentThread.Name ?? "Unknown Thread";
        pThreadId = Hash.Digest(pThreadLockName);
#endif
        pIsLocked = true;
    }

    public void Unlock()
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
#if DEBUG
        pThreadLockName = string.Empty;
        pThreadId = 0;
#endif
        Core.Threading.Monitor.Exit(GetObjectSync());
        pIsLocked = false;
    }

    protected void ValidateLock()
    {
        if (!pIsLocked)
            throw new Exception("Batch must lock first.");
    }

    protected void ValidateDispose()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    protected abstract object GetObjectSync();
    protected abstract void OnDispose();
}

public sealed class Sprite(int id, SpriteSystem system) : BaseSprite(id)
{
    public bool Enabled
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return system.IsEnabled(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            system.SetEnabled(id, value);
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
            return system.GetPosition(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            system.SetPosition(id, value);
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
            return system.GetAnchor(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            system.SetAnchor(id, value);
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
            return system.GetSize(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            system.SetSize(id, value);
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
            return system.GetAngle(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            system.SetAngle(id, value);
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
            return system.GetColor(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            system.SetColor(id, value);
        }
    }

    public SpriteEffect Effect
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return system.GetEffect(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            system.SetEffect(id, value);
        }
    }
    
    protected override object GetObjectSync() => system.GetObjectSync(id);

    protected override void OnDispose()
    {
        system.Destroy(id);
    }
}

public sealed class InstancedSprite(int id, SpriteInstancedBatchSystem batchSystem) : BaseSprite(id)
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

    public InstancedSpriteEffect Effect
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

    public IBuffer InstancingBuffer
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return batchSystem.GetInstancingBuffer(id);
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

    public InstancedSprite SetInstancePosition(uint instanceIdx, Vector2 position)
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

    public InstancedSprite SetInstanceScale(uint instanceIdx, Vector2 scale)
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

    public InstancedSprite SetInstanceAngle(uint instanceIdx, float angle)
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

    public InstancedSprite SetInstanceAnchor(uint instanceIdx, Vector2 anchor)
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

    public InstancedSprite SetInstanceDepth(uint instanceIdx, float depth)
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