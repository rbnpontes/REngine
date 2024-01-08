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

public sealed class Sprite(int id, SpriteRenderSystem renderSystem) : BaseSprite(id)
{
    public bool Enabled
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return renderSystem.IsEnabled(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            renderSystem.SetEnabled(id, value);
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
            return renderSystem.GetPosition(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            renderSystem.SetPosition(id, value);
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
            return renderSystem.GetAnchor(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            renderSystem.SetAnchor(id, value);
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
            return renderSystem.GetSize(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            renderSystem.SetSize(id, value);
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
            return renderSystem.GetAngle(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            renderSystem.SetAngle(id, value);
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
            return renderSystem.GetColor(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            renderSystem.SetColor(id, value);
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
            return renderSystem.GetEffect(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            renderSystem.SetEffect(id, value);
        }
    }
    
    protected override object GetObjectSync() => renderSystem.GetObjectSync(id);

    protected override void OnDispose()
    {
        renderSystem.Destroy(id);
    }
}

public sealed class InstancedSprite(int id, SpriteInstancedRenderSystem renderSystem) : BaseSprite(id)
{
    public bool Enabled
    {
        get
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            return renderSystem.IsEnabled(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            renderSystem.SetEnabled(id, value);
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
            return renderSystem.GetColor(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            renderSystem.SetColor(id, value);
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
            return renderSystem.GetEffect(id);
        }
        set
        {
#if DEBUG
            ValidateDispose();
            ValidateLock();
#endif
            renderSystem.SetEffect(id, value);
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
            return renderSystem.GetInstanceCount(id);
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
            return renderSystem.GetInstancingBuffer(id);
        }
    }

    public void ResizeInstances(uint numInstances, bool dynamic = false)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        renderSystem.ResizeInstances(id, numInstances, dynamic);
    }

    public Vector2 GetInstancePosition(uint instanceIdx)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        renderSystem.GetElement(id, instanceIdx, out var element);
        return element.Position;
    }

    public Vector2 GetInstanceScale(uint instanceIdx)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        renderSystem.GetElement(id, instanceIdx, out var element);
        return element.Scale;
    }

    public float GetInstanceAngle(uint instanceIdx)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        renderSystem.GetElement(id, instanceIdx, out var element);
        return element.Angle;
    }

    public Vector2 GetInstanceAnchor(uint instanceIdx)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        renderSystem.GetElement(id, instanceIdx, out var element);
        return element.Anchor;
    }

    public float GetInstanceDepth(uint instanceIdx)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        renderSystem.GetElement(id, instanceIdx, out var element);
        return element.Depth;
    }

    public InstancedSprite SetInstancePosition(uint instanceIdx, Vector2 position)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        renderSystem.GetElement(id, instanceIdx, out var element);
        element.Position = position;
        renderSystem.SetElement(id, instanceIdx, ref element);
        return this;
    }

    public InstancedSprite SetInstanceScale(uint instanceIdx, Vector2 scale)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        renderSystem.GetElement(id, instanceIdx, out var element);
        element.Scale = scale;
        renderSystem.SetElement(id, instanceIdx, ref element);
        return this;
    }

    public InstancedSprite SetInstanceAngle(uint instanceIdx, float angle)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        renderSystem.GetElement(id, instanceIdx, out var element);
        element.Angle = angle;
        renderSystem.SetElement(id, instanceIdx, ref element);
        return this;
    }

    public InstancedSprite SetInstanceAnchor(uint instanceIdx, Vector2 anchor)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        renderSystem.GetElement(id, instanceIdx, out var element);
        element.Anchor = anchor;
        renderSystem.SetElement(id, instanceIdx, ref element);
        return this;
    }

    public InstancedSprite SetInstanceDepth(uint instanceIdx, float depth)
    {
#if DEBUG
        ValidateDispose();
        ValidateLock();
#endif
        renderSystem.GetElement(id, instanceIdx, out var element);
        element.Depth = depth;
        renderSystem.SetElement(id, instanceIdx, ref element);
        return this;
    }

    protected override object GetObjectSync() => renderSystem.GetSyncObject(id);

    protected override void OnDispose()
    {
        renderSystem.DestroyBatch(id);
    }
}