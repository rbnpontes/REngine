using System.Drawing;
using System.Numerics;
using REngine.Core.Mathematics;
using REngine.RHI;

namespace REngine.RPI.Batches;

public struct SpriteBatchItemDesc
{
    public bool Enabled;
    public Vector3 Position;
    public Vector2 Anchor;
    public Vector2 Scale;
    public float Rotation;
    public Color Color;
    public SpriteEffect? Effect;
}

public sealed class SpriteBatchItem(
    IBufferManager bufferManager, 
    ISpriteBatch spriteBatch,
    GraphicsBackend backend) : Batch
{
    private struct InternalData()
    {
        public Vector3 Position = Vector3.Zero;
        public Vector2 RotationCoeffiecients = Vector2.Zero;
        public Vector2 Scale = Vector2.One;
        public Vector2 Anchor = Vector2.Zero;
        public Vector4 Color = Vector4.One;
    }

    private readonly object pSync = new();

    private bool pEnabled;
    private InternalData pData = new();

    private IShaderResourceBinding? pShaderResourceBinding;
    private IPipelineState? pPipelineState;

    public override int GetSortIndex()
    {
        lock (pSync)
            return Mathf.FloatToInt(pData.Position.Z);
    }

    public void Update(in SpriteBatchItemDesc desc)
    {
        lock (pSync)
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
            
            pEnabled = desc.Enabled;
            pPipelineState = desc.Effect?.OnBuildPipeline();
            pShaderResourceBinding = desc.Effect?.OnGetShaderResourceBinding();
            pData = new InternalData()
            {
                Color = desc.Color.ToVector4(),
                Position = position,
                RotationCoeffiecients = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation)),
                Scale = scale,
                Anchor = anchor
            };
        }
    }

    public override void Render(BatchRenderInfo batchRenderInfo)
    {
        bool enabled;
        InternalData data;
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

        var cbuffer = bufferManager.GetBuffer(BufferGroupType.Object);
        var command = batchRenderInfo.CommandBuffer;

        // Update Constant Buffer
        var mapped = command.Map<InternalData>(cbuffer, MapType.Write, MapFlags.Discard);
        mapped[0] = data;
        command.Unmap(cbuffer, MapType.Write);

        command
            .SetPipeline(pipelineState)
            .CommitBindings(shaderResourceBinding)
            .Draw(new DrawArgs()
            {
                NumInstances = 1,
                NumVertices = 4
            });
    }


    protected override void OnDispose()
    {
        spriteBatch.RemoveBatch(this);
    }
}