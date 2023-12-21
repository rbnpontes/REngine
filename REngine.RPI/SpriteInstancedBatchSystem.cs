using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using REngine.Core.Mathematics;
using REngine.Core.Utils;
using REngine.Core.WorldManagement;
using REngine.RHI;
using REngine.RPI.Structs;

namespace REngine.RPI;

public sealed class SpriteInstancedBatchSystem(
    RenderSettings renderSettings,
    IBufferManager bufferManager) : BaseSystem<SpriteInstanceBatchItem>
{
    private readonly object pSync = new();

    public SpriteInstanceBatch CreateBatch(int numInstances = 0, bool dynamic = false)
    {
        SpriteInstanceBatch batch;
        lock (pSync)
        {
            var id = Acquire();
            batch = new SpriteInstanceBatch(id, this);
            pData[id] = new SpriteInstanceBatchItem(
                bufferManager.GetInstancingBuffer((ulong)(numInstances * Unsafe.SizeOf<Matrix4x4>()), dynamic))
            {
                RefBatch = batch,
                Items = new SpriteInstanceBatchElement[numInstances],
            };
        }

        return batch;
    }

    public void DestroyBatch(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].RefBatch?.Dispose();
            pData[id].RefBatch = null;
            pData[id].ShaderResourceBinding?.Dispose();
            pData[id].InstanceBuffer.Dispose();
            pAvailableIdx.Enqueue(id);
        }
    }

    public object GetSyncObject(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Sync;
        }
    }

    public SpriteInstancedBatchSystem ResizeInstances(int id, uint numInstances, bool dynamic = false)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            if (pData[id].Items.Length == numInstances &&
                pData[id].InstanceBuffer.Desc.Usage == (dynamic ? Usage.Dynamic : Usage.Default))
                return this;

            pData[id].InstanceBuffer.Dispose();
            pData[id].InstanceBuffer =
                bufferManager.GetInstancingBuffer((ulong)(numInstances * Unsafe.SizeOf<Matrix4x4>()), dynamic);
            pData[id].Items = new SpriteInstanceBatchElement[numInstances];
            pData[id].Transforms = [];
            pData[id].DirtyInstances = true;
        }

        return this;
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

    public int GetInstanceCount(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Items.Length;
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

    public IBuffer GetInstancingBuffer(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].InstanceBuffer;
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

    public bool IsDirtyInstances(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].DirtyInstances;
        }
    }

    public void GetElement(int id, uint elementIdx, out SpriteInstanceBatchElement element)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            var data = pData[id];
#if DEBUG
            ExceptionUtils.ThrowIfOutOfBounds((int)elementIdx, data.Items.Length);
#endif
            element = data.Items[elementIdx];
        }
    }

    public void GetTransforms(int id, out Matrix3x3[] transforms)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            transforms = pData[id].Transforms;
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

    public void SetShaderResourceBinding(int id, IShaderResourceBinding srb)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].ShaderResourceBinding?.Dispose();
            pData[id].ShaderResourceBinding = srb;
            pData[id].Dirty = true;
        }
    }

    public void SetPipelineState(int id, IPipelineState? pipeline)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].PipelineState = pipeline;
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

    public void RemoveDirtyInstancesState(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].DirtyInstances = false;
        }
    }

    public void SetElement(int id, uint elementIdx, ref SpriteInstanceBatchElement element)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            var data = pData[id];
#if DEBUG
            ExceptionUtils.ThrowIfOutOfBounds((int)elementIdx, data.Items.Length);
#endif
            data.DirtyInstances = true;
            data.Items[elementIdx] = element;
        }
    }

    public SpriteInstanceBatchElement[] GetElements(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Items;
        }
    }

    public void UpdateTransforms()
    {
        lock (pSync)
        {
            if (pAvailableIdx.Count == pData.Length)
                return;

            for (var i = 0; i < pData.Length; ++i)
            {
                var data = pData[i];
                if (data.RefBatch is null)
                    continue;
                if (!data.DirtyInstances)
                    continue;

                var transforms = data.Transforms.Length != data.Items.Length ? new Matrix3x3[data.Items.Length] : data.Transforms;

                for (var j = 0; j < transforms.Length; ++j)
                    transforms[j] = GetCompactedMatrix(ref data.Items[j]);
                    
                pData[i].Transforms = transforms;
            }
        }

        return;

        // This matrix does not represent an Transformation Matrix
        // This matrix only compacts essential values to be transformed on
        // Transform Matrix4x4 on shader
        Matrix3x3 GetCompactedMatrix(ref SpriteInstanceBatchElement element)
        {
            return new Matrix3x3(
                element.Position.X, element.Position.Y, 0/*element.Position.Z*/,
                (float)Math.Cos(element.Angle), (float)Math.Sin(element.Angle), element.Anchor.X,
                element.Scale.X, element.Scale.Y, element.Anchor.Y
            );
        }
    }

    public void ClearTransforms(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Transforms = [];
        }
    }

    public void ForEach(Action<SpriteInstanceBatch> callback)
    {
        lock (pSync)
        {
            if (pAvailableIdx.Count == pData.Length)
                return;
            foreach (var data in pData)
            {
                if (data.RefBatch is null)
                    continue;
                callback(data.RefBatch);
            }
        }
    }

    protected override int GetExpansionSize()
    {
        return (int)Math.Round(
            Math.Max(
                (float)renderSettings.SpriteBatchInitialInstanceSize * renderSettings.SpriteBatchInstanceExpansionRatio,
                renderSettings.SpriteBatchInitialInstanceSize
            )
        );
    }
}