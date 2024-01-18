using System.Buffers;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using REngine.Core;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI.Effects;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace REngine.RPI.Batches;

public struct SpriteInstanceBatchItemDesc()
{
    public bool Enabled;
    public Matrix4x4 Transform = Matrix4x4.Identity;
    public SpriteEffect? Effect;
    public SpriteBatchItem[] Items = [];
}

public abstract class SpriteInstanceBatch(
    ISpriteBatch spriteBatch,
    IBufferManager bufferManager
) : Batch
{
    protected static readonly ArrayPool<SpriteBatchData> DataPool = ArrayPool<SpriteBatchData>.Shared;
    protected readonly object mSync = new();

    protected int mZIndex;
    protected uint mInstanceCount;
    protected Matrix4x4 mTransform = Matrix4x4.Identity;
    protected SpriteBatchData[] mData = [];
    protected bool mEnabled;
    protected IShaderResourceBinding? mShaderResourceBinding;
    protected IPipelineState? mPipelineState;
    protected IBuffer? mBuffer;
    protected ulong mRequiredBufferSize;

    public override int GetSortIndex()
    {
        lock (mSync)
            return mZIndex;
    }

    protected override void OnDispose()
    {
        DisposableQueue.Enqueue(mBuffer);
        if (mData.Length != 0)
            DataPool.Return(mData);
        spriteBatch.RemoveBatch(this);
    }

    public virtual void Update(SpriteInstanceBatchItemDesc desc)
    {
        lock (mSync)
        {
            // if items count is less than temp data
            // we need to acquire a new array from pool
            if (mInstanceCount < desc.Items.Length && desc.Items.Length != 0)
            {
                if (mInstanceCount != 0)
                    DataPool.Return(mData);
                mData = DataPool.Rent(desc.Items.Length);
            }

            mEnabled = desc.Enabled;
            mPipelineState = desc.Effect?.BuildPipeline2();
            mShaderResourceBinding = desc.Effect?.OnGetShaderResourceBinding();
            mInstanceCount = (uint)desc.Items.Length;
            if (desc.Items.Length == 0)
                mEnabled = false;

            mZIndex = Mathf.FloatToInt(desc.Items[0].Position.Z);
            mRequiredBufferSize = (ulong)Unsafe.SizeOf<SpriteBatchData>() * mInstanceCount;
            mTransform = desc.Transform;

            var idx = 0;
            while (idx < desc.Items.Length)
            {
                SpriteBatch.BuildBatchData(desc.Items[idx], out mData[idx]);
                ++idx;
            }
        }
    }

    public override void Render(BatchRenderInfo batchRenderInfo)
    {
        bool enabled;
        uint instanceCount;
        SpriteBatchData[] data;
        IPipelineState? pipelineState;
        IShaderResourceBinding? shaderResourceBinding;
        Matrix4x4 transform;

        lock (mSync)
        {
            enabled = mEnabled;
            data = mData;
            instanceCount = mInstanceCount;
            pipelineState = mPipelineState;
            shaderResourceBinding = mShaderResourceBinding;
            transform = mTransform;
        }

        if (!enabled || pipelineState is null || shaderResourceBinding is null)
            return;

        var command = batchRenderInfo.CommandBuffer;

        OnBeforeRender(batchRenderInfo, data, instanceCount);

        if (mBuffer is null)
            return;

        {
            // Update Constant Buffer
            var cbuffer = bufferManager.GetBuffer(BufferGroupType.Object);
            var mappedData = command.Map<Matrix4x4>(cbuffer, MapType.Write, MapFlags.Discard);
            mappedData[0] = mTransform;
            command.Unmap(cbuffer, MapType.Write);
        }

        command
            .SetVertexBuffer(mBuffer)
            .SetPipeline(pipelineState)
            .CommitBindings(shaderResourceBinding)
            .Draw(new DrawArgs()
            {
                NumInstances = mInstanceCount,
                NumVertices = 4
            });
    }

    protected abstract void OnBeforeRender(BatchRenderInfo batchRenderInfo, SpriteBatchData[] data, uint instanceCount);
}

/// <summary>
/// Use this batch if you want to update
/// batches with high frequency
/// Internally, a dynamic buffer will be used
/// to optimize this changes
/// </summary>
/// <param name="bufferManager"></param>
public sealed class DynamicSpriteInstanceBatch(
    IBufferManager bufferManager,
    ISpriteBatch spriteBatch
) : SpriteInstanceBatch(spriteBatch, bufferManager)
{
    protected override unsafe void OnBeforeRender(BatchRenderInfo batchRenderInfo, SpriteBatchData[] data,
        uint instanceCount)
    {
        mBuffer ??= bufferManager.GetInstancingBuffer(mRequiredBufferSize, true);
        if (mBuffer.Size < mRequiredBufferSize)
        {
            DisposableQueue.Enqueue(mBuffer);
            mBuffer = bufferManager.GetInstancingBuffer(mRequiredBufferSize, true);
        }

        // Update Instance Buffer
        var command = batchRenderInfo.CommandBuffer;
        var gpuPtr = command.Map(mBuffer, MapType.Write, MapFlags.Discard);
        fixed (void* copyPtr = data)
            Buffer.MemoryCopy(copyPtr, gpuPtr.ToPointer(), mRequiredBufferSize, mRequiredBufferSize);
        command.Unmap(mBuffer, MapType.Write);
    }
}

/// <summary>
/// Instead of <see cref="DynamicSpriteInstanceBatch"/>
/// This batch is recommended to occasionally updates
/// Is not recommended to update this batch frequently
/// In this case, use <see cref="DynamicSpriteInstanceBatch"/>
/// </summary>
/// <param name="bufferManager"></param>
public sealed class DefaultSpriteInstanceBatch(
    IBufferManager bufferManager,
    ISpriteBatch spriteBatch
) : SpriteInstanceBatch(spriteBatch, bufferManager)
{
    private bool pDirtyBuffer;

    public override void Update(SpriteInstanceBatchItemDesc desc)
    {
        var currInstanceCount = mInstanceCount;
        base.Update(desc);
        pDirtyBuffer |= currInstanceCount != mInstanceCount;
    }

    protected override unsafe void OnBeforeRender(BatchRenderInfo batchRenderInfo, SpriteBatchData[] data,
        uint instanceCount)
    {
        if (mBuffer is null)
            pDirtyBuffer = true;

        mBuffer ??= bufferManager.GetInstancingBuffer(mRequiredBufferSize, false);
        if (mBuffer.Size < mRequiredBufferSize)
        {
            DisposableQueue.Enqueue(mBuffer);
            mBuffer = bufferManager.GetInstancingBuffer(mRequiredBufferSize, false);
            pDirtyBuffer = true;
        }

        if (!pDirtyBuffer)
            return;

        var command = batchRenderInfo.CommandBuffer;
        fixed (void* copyPtr = data)
            command.UpdateBuffer(mBuffer, 0, mRequiredBufferSize, new IntPtr(copyPtr));
    }
}

/// <summary>
/// Instead of <see cref="DynamicSpriteInstanceBatch"/> or <see cref="DefaultSpriteInstanceBatch"/>
/// This object is not recommended to any case of update
/// If this batch goes to update, then a new buffer will be created
/// Use this object if you have objects that does not change along of the frame
/// </summary>
/// <param name="bufferManager"></param>
public sealed class StaticSpriteInstanceBatch(
    IBufferManager bufferManager,
    ISpriteBatch spriteBatch
) : SpriteInstanceBatch(spriteBatch, bufferManager)
{
    private bool pDirtyBuffer;

    public override void Update(SpriteInstanceBatchItemDesc desc)
    {
        var currInstanceCount = mInstanceCount;
        base.Update(desc);
        pDirtyBuffer |= currInstanceCount != mInstanceCount;
    }

    protected override void OnBeforeRender(BatchRenderInfo batchRenderInfo, SpriteBatchData[] data, uint instanceCount)
    {
        if (pDirtyBuffer)
        {
            DisposableQueue.Enqueue(mBuffer);
            mBuffer = null;
        }

        mBuffer ??= AllocateInstancingBuffer(data, instanceCount);

        if (mBuffer.Size >= mRequiredBufferSize)
            return;

        DisposableQueue.Enqueue(mBuffer);
        mBuffer = AllocateInstancingBuffer(data, instanceCount);
    }

    private IBuffer AllocateInstancingBuffer(SpriteBatchData[] data, uint instanceCount)
    {
        var bufferData = new SpriteBatchData[instanceCount];
        Array.Copy(data, bufferData, instanceCount);
        var result = bufferManager.Allocate(new BufferDesc()
        {
            Name = "Instancing Buffer Static",
            Size = mRequiredBufferSize,
            Usage = Usage.Immutable,
            AccessFlags = CpuAccessFlags.None,
            BindFlags = BindFlags.ShaderResource
        }, bufferData);

        DataPool.Return(mData);
        mData = [];
        return result;
    }
}