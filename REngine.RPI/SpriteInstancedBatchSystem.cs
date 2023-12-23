using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using REngine.Core;
using REngine.Core.Mathematics;
using REngine.Core.Utils;
using REngine.Core.WorldManagement;
using REngine.RHI;
using REngine.RPI.Events;
using REngine.RPI.Structs;

namespace REngine.RPI;

public sealed class SpriteInstancedBatchSystem(
    RendererEvents rendererEvents,
    RenderSettings renderSettings,
    BatchSystem batchSystem,
    IBufferManager bufferManager,
    IServiceProvider provider)
    : BaseSystem<SpriteInstanceBatchItem>((int)renderSettings.SpriteBatchInitialInstanceSize)
{
    private class InternalBatch(SpriteInstancedBatchSystem system, int id, IBuffer constantBuffer) : QuadBatch
    {
        public override unsafe void Render(ICommandBuffer command)
        {
            InstancedSprite? sprite;
            lock (system.pSync)
                sprite = system.pData[id].RefSprite;
            if (sprite is null)
                return;
            
            sprite.Lock();
            // Skip rendering if sprite is enabled
            if (!sprite.Enabled)
            {
                sprite.Unlock();
                return;
            }

            var color = sprite.Color;
            var instancingBuffer = system.GetInstancingBuffer(id);
            var isDirtyInstances = system.IsDirtyInstances(id);
            system.GetTransforms(id, out var transforms);
            var effect = sprite.Effect;
            sprite.Unlock();

            PipelineState = effect.OnBuildPipeline();
            ShaderResourceBinding = effect.OnGetShaderResourceBinding();

            // If has zero transforms, then we must skip render
            if (transforms.Length == 0)
                return;

            NumInstances = (uint)transforms.Length;
            effect.UpdateBuffers();
            
            if (instancingBuffer.Desc.Usage == Usage.Dynamic)
            {
                var ptr = command.Map(instancingBuffer, MapType.Write, MapFlags.Discard);
                var size = (ulong)(Unsafe.SizeOf<Matrix3x3>() * transforms.Length);
                fixed(Matrix3x3* transformsPtr = transforms)
                    Buffer.MemoryCopy(transformsPtr, ptr.ToPointer(), size, size);
                command.Unmap(instancingBuffer, MapType.Write);
            }
            else if (isDirtyInstances)
            {
                command.UpdateBuffer(instancingBuffer, 0, new ReadOnlySpan<Matrix3x3>(transforms));
                system.ClearTransforms(id);
                system.RemoveDirtyInstancesState(id);
            }

            var mappedData = command.Map<Vector4>(constantBuffer, MapType.Write, MapFlags.Discard);
            mappedData[0] = color.ToVector4();
            command
                .Unmap(constantBuffer, MapType.Write)
                .SetVertexBuffer(instancingBuffer);
            base.Render(command);
        }
    }
    
    private readonly object pSync = new();
    private readonly BatchGroup pBatchGroup = batchSystem.GetGroup(SpriteSystem.BatchGroupName);
    private readonly InstancedSpriteEffect pDefaultEffect = InstancedSpriteEffect.Build(provider);

    public InstancedSprite CreateBatch(uint numInstances = 0, InstancedSpriteEffect? spriteEffect = null, bool dynamic = false)
    {
        InstancedSprite sprite;
        lock (pSync)
        {
            var id = Acquire();
            var batch = new InternalBatch(this, id, bufferManager.GetBuffer(BufferGroupType.Object));
            
            sprite = new InstancedSprite(id, this);
            spriteEffect ??= pDefaultEffect;
            
            pData[id] = new SpriteInstanceBatchItem(
                bufferManager.GetInstancingBuffer((ulong)(numInstances * Unsafe.SizeOf<Matrix4x4>()), dynamic), 
                spriteEffect
            )
            {
                RefSprite = sprite,
                BatchIndex = pBatchGroup.AddBatch(batch),
                Items = new SpriteInstanceBatchElement[numInstances],
            };
        }

        return sprite;
    }
    public void DestroyBatch(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            var data = pData[id];
            if (data.RefSprite is null)
                return;
            data.RefSprite?.Dispose();
            data.RefSprite = null;
            data.InstanceBuffer.Dispose();
            pBatchGroup.RemoveBatch(pData[id].BatchIndex);
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

    public void ResizeInstances(int id, uint numInstances, bool dynamic = false)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            if (pData[id].Items.Length == numInstances &&
                pData[id].InstanceBuffer.Desc.Usage == (dynamic ? Usage.Dynamic : Usage.Default))
                return;

            pData[id].InstanceBuffer.Dispose();
            pData[id].InstanceBuffer =
                bufferManager.GetInstancingBuffer((ulong)(numInstances * Unsafe.SizeOf<Matrix4x4>()), dynamic);
            pData[id].Items = new SpriteInstanceBatchElement[numInstances];
            pData[id].Transforms = [];
            pData[id].DirtyInstances = true;
        }
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

    public InstancedSpriteEffect GetEffect(int id)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            return pData[id].Effect;
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

    public void SetEffect(int id, InstancedSpriteEffect effect)
    {
        lock (pSync)
        {
#if DEBUG
            ValidateId(id);
#endif
            pData[id].Effect = effect;
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
                if (data.RefSprite is null)
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

    public void ForEach(Action<InstancedSprite> callback)
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