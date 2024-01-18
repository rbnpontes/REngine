using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using REngine.Core;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI.Effects;

namespace REngine.RPI.Batches;

public struct SpriteBatchItem
{
    public Vector3 Position;
    public Vector2 Anchor;
    public Vector2 Scale;
    public float Rotation;
    public Color Color;
}

public struct SpriteBatchItemDesc
{
    public bool Enabled;
    public SpriteEffect? Effect;
    public SpriteBatchItem Item;
}

public struct SpriteBatchData()
{
    public Vector3 Position = Vector3.Zero;
    public Vector2 RotationCoeffiecients = Vector2.Zero;
    public Vector2 Scale = Vector2.One;
    public Vector2 Anchor = Vector2.Zero;
    public Vector4 Color = Vector4.One;
    public Vector3 UnusedData = Vector3.Zero;
}

public sealed class SpriteBatch(
    IBufferManager bufferManager,
    ISpriteBatch spriteBatch) : Batch
{
    private static RefCount<IBuffer>? sInstanceBuffer;
    private readonly object pSync = new();

    private bool pEnabled;
    private SpriteBatchData pData = new();

    private IShaderResourceBinding? pShaderResourceBinding;
    private IPipelineState? pPipelineState;
    private IBuffer? pBuffer;

    public override int GetSortIndex()
    {
        lock (pSync)
            return Mathf.FloatToInt(pData.Position.Z);
    }

    public void Update(in SpriteBatchItemDesc desc)
    {
        lock (pSync)
        {
            pEnabled = desc.Enabled;
            pPipelineState = desc.Effect?.BuildPipeline2();
            pShaderResourceBinding = desc.Effect?.OnGetShaderResourceBinding();

            BuildBatchData(desc.Item, out pData);
        }
    }

    public override void Render(BatchRenderInfo batchRenderInfo)
    {
        bool enabled;
        SpriteBatchData data;
        IPipelineState? pipelineState;
        IShaderResourceBinding? shaderResourceBinding;

        lock (pSync)
        {
            enabled = pEnabled;
            data = pData;
            pipelineState = pPipelineState;
            shaderResourceBinding = pShaderResourceBinding;
        }

        if (!enabled || pPipelineState is null || pShaderResourceBinding is null)
            return;

        var command = batchRenderInfo.CommandBuffer;
        var buffer = GetBuffer();
        {
            // Update Instancing Buffer
            var mapped = command.Map<SpriteBatchData>(buffer, MapType.Write, MapFlags.Discard);
            mapped[0] = data;
            command.Unmap(buffer, MapType.Write);
        }

        {
            // Update Constant Buffer
            var cbuffer = bufferManager.GetBuffer(BufferGroupType.Object);
            var mapped = command.Map<Matrix4x4>(cbuffer, MapType.Write, MapFlags.Discard);
            mapped[0] = Matrix4x4.Identity;
            command.Unmap(cbuffer, MapType.Write);
        }
        
        command
            .SetVertexBuffer(buffer)
            .SetPipeline(pipelineState)
            .CommitBindings(shaderResourceBinding)
            .Draw(new DrawArgs()
            {
                NumInstances = 1,
                NumVertices = 4
            });
    }

    private IBuffer GetBuffer()
    {
        if (pBuffer is not null)
            return pBuffer;
        if (sInstanceBuffer is not null && sInstanceBuffer.Count > 0)
        {
            pBuffer = sInstanceBuffer.Ref;
            sInstanceBuffer.AddRef();
            return pBuffer;
        }

        var buffer = bufferManager.GetInstancingBuffer((ulong)Unsafe.SizeOf<SpriteBatchData>(), true);
        sInstanceBuffer = new RefCount<IBuffer>(buffer);
        pBuffer = buffer;
        return buffer;
    }

    protected override void OnDispose()
    {
        DisposableQueue.Enqueue(sInstanceBuffer);
        if (sInstanceBuffer?.Count == 1)
            sInstanceBuffer = null;
        spriteBatch.RemoveBatch(this);
    }

    public static void BuildBatchData(in SpriteBatchItem desc, out SpriteBatchData data)
    {
        var position = desc.Position;
        var scale = desc.Scale;
        var anchor = desc.Anchor;
        var offset = anchor * scale;

        scale.Y = -scale.Y;
        anchor.Y -= 1.0f;
        anchor.X = -anchor.X;

        position.X += offset.X;
        position.Y += offset.Y;

        var rotation = desc.Rotation;
        position.Z = Math.Clamp(position.Z / ushort.MaxValue, 0, 1);

        data = new SpriteBatchData()
        {
            Color = desc.Color.ToVector4(),
            Position = position,
            RotationCoeffiecients = new Vector2(
                (float)Math.Cos(rotation),
                (float)Math.Sin(rotation)
            ),
            Scale = scale,
            Anchor = anchor
        };
    }
}