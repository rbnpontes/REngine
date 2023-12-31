using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.Core.Threading;
using REngine.Core.Utils;
using REngine.Core.WorldManagement;
using REngine.RHI;
using REngine.RPI.Events;
using REngine.RPI.Structs;
using REngine.RPI.Utils;
using SpriteInstanceData = System.Numerics.Matrix4x4;
namespace REngine.RPI;

public struct SpriteInstancedCreateInfo()
{
    public uint NumInstances = 1;
    public bool Dynamic = false;
    public IBuffer? InstancingBuffer = null;
    public InstancedSpriteEffect? Effect = null;
}
public sealed class SpriteInstancedBatchSystem(
    RendererEvents rendererEvents,
    RenderSettings renderSettings,
    BatchSystem batchSystem,
    IBufferManager bufferManager,
    IServiceProvider provider,
    RenderState renderState,
    IExecutionPipeline executionPipeline)
    : BaseSystem<SpriteInstanceBatchItem>((int)renderSettings.SpriteBatchInitialInstanceSize)
{
    public static readonly ulong InstanceDataSize = (ulong)Unsafe.SizeOf<SpriteInstanceData>();
    private class InternalBatch(
        SpriteInstancedBatchSystem system, 
        int id, 
        IBuffer constantBuffer,
        IExecutionPipeline execPipeline,
        IGraphicsDriver driver,
        RenderSettings renderSettings) : Batch
    {
        private readonly ManualResetEventSlim pManualResetEventSlim = new(false);
        private ICommandList[] pCommandLists = [];
        private int pFinishedJobs = 0;
        private IBuffer? pInstancingBuffer;
        
        
        public override unsafe void Render(BatchRenderInfo batchRenderInfo)
        {
            var command = batchRenderInfo.CommandBuffer;
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

            var bufferType = system.pData[id].BufferType;
            var color = sprite.Color;
            var instancingBuffer = system.GetInstancingBuffer(id);
            var isDirtyInstances = system.IsDirtyInstances(id);
            system.GetTransforms(id, out var transforms);
            var effect = sprite.Effect;
            sprite.Unlock();

            PipelineState = effect.OnBuildPipeline();
            ShaderResourceBinding = effect.OnGetShaderResourceBinding();

            // If has zero transforms, then we must skip render
            if (transforms.Length == 0 || PipelineState is null || ShaderResourceBinding is null)
                return;

            var numInstances = (uint)transforms.Length;
            effect.UpdateBuffers();
            
            switch (bufferType)
            {
                case SpriteBufferType.Dynamic:
                {
                    var ptr = command.Map(instancingBuffer, MapType.Write, MapFlags.Discard);
                    var size = InstanceDataSize * (ulong)transforms.Length;
                    fixed(SpriteInstanceData* transformsPtr = transforms)
                        Buffer.MemoryCopy(transformsPtr, ptr.ToPointer(), size, size);
                    command.Unmap(instancingBuffer, MapType.Write);
                    break;
                }
                case SpriteBufferType.Default:
                    if (isDirtyInstances)
                    {
                        command.UpdateBuffer(instancingBuffer, 0, new ReadOnlySpan<SpriteInstanceData>(transforms));
                        system.ClearTransforms(id);
                        system.RemoveDirtyInstancesState(id);
                    }
                    break;
                // External buffer is handled externally
                case SpriteBufferType.External:
                default:
                    break;
            }

            var jobsCount = GetJobsCount();
            var mappedData = command.Map<Vector4>(constantBuffer, MapType.Write, MapFlags.Discard);
            mappedData[0] = color.ToVector4();
            command.Unmap(constantBuffer, MapType.Write);

            if (driver.Backend == GraphicsBackend.OpenGL 
                || jobsCount == 0 
                || numInstances == jobsCount
                || !renderSettings.EnableSpriteBatchMultiThread)
            {
                command
                    .SetVertexBuffer(instancingBuffer, false)
                    .SetPipeline(PipelineState)
                    .CommitBindings(ShaderResourceBinding)
                    .Draw(new DrawArgs()
                    {
                        NumInstances = numInstances,
                        NumVertices = 4
                    });
                return;
            }

            command.TransitionShaderResource(PipelineState, ShaderResourceBinding);
            pFinishedJobs = 0;
            pManualResetEventSlim.Reset();

            if (jobsCount != pCommandLists.Length)
                pCommandLists = new ICommandList[jobsCount];
            
            var commands = driver.Commands;

            pInstancingBuffer = instancingBuffer;
            for (var i = 0; i < jobsCount; ++i)
            {
                var jobCommand = commands[i];
                var jobIdx = i;
                execPipeline.Schedule(() => RenderSubset(jobCommand, jobIdx, numInstances, batchRenderInfo));
            }

            // Wait all jobs to finish
            while (pFinishedJobs < jobsCount) {}

            command.ExecuteCommandList(pCommandLists);
            foreach(var cmd in pCommandLists)
                cmd.Dispose();
            
            pManualResetEventSlim.Set();
        }

        private byte GetJobsCount()
        {
            return (byte)Math.Clamp(renderSettings.SpriteBatchInstanceJobs, 1, execPipeline.JobsCount);
        }
        private void RenderSubset(ICommandBuffer command, int jobIdx, uint numInstances, BatchRenderInfo batchRenderInfo)
        {
            if (pInstancingBuffer is null)
                return;

            var jobsCount = GetJobsCount();
            var instancesPerJob = numInstances / jobsCount;
            var instanceOffset = (int)((jobIdx / (float)jobsCount) * numInstances);
            var numSubsets = renderSettings.SpriteBatchInstanceSubset;
            
            command
                .Begin(0)
                .SetRT(batchRenderInfo.DefaultRenderTarget, batchRenderInfo.DefaultDepthStencil);
            
            var offsets = new[] { (ulong)(instanceOffset * Unsafe.SizeOf<SpriteInstanceData>()) };
            var offsetInstance = 0u;
            while (offsetInstance < instancesPerJob)
            {
                command
                    .SetVertexBuffers(0, new IBuffer[] { pInstancingBuffer }, offsets, false)
                    .SetPipeline(PipelineState)
                    .CommitBindings(ShaderResourceBinding)
                    .Draw(new DrawArgs()
                    {
                        NumInstances = (uint)Math.Min(numSubsets, offsetInstance - instancesPerJob),
                        NumVertices = 4
                    });
                offsets[0] += (ulong)(numSubsets * Unsafe.SizeOf<SpriteInstanceData>());
                offsetInstance += numSubsets;
            }
            
            command.FinishCommandList(out var commandList);
            pCommandLists[jobIdx] = commandList;

            Interlocked.Increment(ref pFinishedJobs);
            
            // Wait for main thread to execute command list
            pManualResetEventSlim.Wait();
            
            command.FinishFrame();
        }
    }
    
    private readonly object pSync = new();
    private readonly BatchGroup pBatchGroup = batchSystem.GetGroup(SpriteSystem.BatchGroupName);
    private readonly InstancedSpriteEffect pDefaultEffect = InstancedSpriteEffect.Build(provider);

    public InstancedSprite CreateBatch(SpriteInstancedCreateInfo createInfo)
    {
        InstancedSprite sprite;
        lock (pSync)
        {
            SpriteBufferType bufferType;
            
            var instancingBuffer = createInfo.InstancingBuffer;
            if (instancingBuffer != null)
            {
                var requiredSize = (ulong)(createInfo.NumInstances * Unsafe.SizeOf<Matrix3x3>());
                if (instancingBuffer.Size < requiredSize)
                    throw new ArgumentOutOfRangeException(
                        $"InstancingBuffer does not match required size. Buffer Length={instancingBuffer.Size}, Required Size={requiredSize}");
                bufferType = SpriteBufferType.External;
            }
            else
            {
                instancingBuffer =
                    bufferManager.GetInstancingBuffer((ulong)(createInfo.NumInstances * Unsafe.SizeOf<Matrix4x4>()), createInfo.Dynamic);
                bufferType = createInfo.Dynamic ? SpriteBufferType.Dynamic : SpriteBufferType.Default;
            }

            var driver = provider.Get<IGraphicsDriver>();
            var id = Acquire();
            var batch = new InternalBatch(
                this, 
                id, 
                bufferManager.GetBuffer(BufferGroupType.Object),
                executionPipeline,
                driver,
                provider.Get<RenderSettings>());
            var effect = createInfo.Effect ?? pDefaultEffect;
            
            sprite = new InstancedSprite(id, this);
            
            pData[id] = new SpriteInstanceBatchItem(
                instancingBuffer, 
                effect
            )
            {
                BufferType = bufferType,
                RefSprite = sprite,
                BatchIndex = pBatchGroup.AddBatch(batch),
                Items = new SpriteInstanceBatchElement[createInfo.NumInstances],
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
            if (pData[id].BufferType == SpriteBufferType.External)
                throw new Exception("Is not possible to resize instances because this sprite uses an External Buffer");

            var bufferType = dynamic ? SpriteBufferType.Dynamic : SpriteBufferType.Default;
            if (pData[id].Items.Length == numInstances && pData[id].BufferType == bufferType)
                return;
            
            pData[id].InstanceBuffer.Dispose();
            pData[id].InstanceBuffer =
                bufferManager.GetInstancingBuffer((ulong)(numInstances * Unsafe.SizeOf<SpriteInstanceData>()), dynamic);
            pData[id].Items = new SpriteInstanceBatchElement[numInstances];
            pData[id].BufferType = bufferType;
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

    public void GetTransforms(int id, out SpriteInstanceData[] transforms)
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

                var transforms = data.Transforms.Length != data.Items.Length ? new SpriteInstanceData[data.Items.Length] : data.Transforms;

                for (var j = 0; j < transforms.Length; ++j)
                    transforms[j] = GetTransformMatrix(ref data.Items[j], renderState.FrameData.ScreenProjection);
                    
                pData[i].Transforms = transforms;
            }
        }

        return;

        // This matrix does not represent an Transformation Matrix
        // This matrix only compacts essential values to be transformed on
        // Transform Matrix4x4 on shader
        // Matrix3x3 GetCompactedMatrix(ref SpriteInstanceBatchElement element)
        // {
        //     return new Matrix3x3(
        //         element.Position.X, element.Position.Y, 0/*element.Position.Z*/,
        //         (float)Math.Cos(element.Angle), (float)Math.Sin(element.Angle), element.Anchor.X,
        //         element.Scale.X, element.Scale.Y, element.Anchor.Y
        //     );
        // }
        SpriteInstanceData GetTransformMatrix(ref SpriteInstanceBatchElement element, Matrix4x4 projection)
        {
            var transform = MatrixUtils.GetSpriteTransform(
                element.Position.ToVector3(),
                element.Anchor,
                element.Angle,
                element.Scale);
            //transform = SpriteInstanceData.Transpose(transform);
            return transform * projection;
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