using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using REngine.Core.Mathematics;
using REngine.RHI;

namespace REngine.RPI.Structs;

public struct SpriteBatchItem()
{
    public object Sync = new();
    public Vector3 Position = Vector3.Zero;
    public Vector2 Anchor = Vector2.Zero;
    public Vector2 Offset = Vector2.Zero;
    public Vector2 Size = Vector2.One;
    public Color Color = Color.Black;
    public float Angle = 0f;
    public bool Enabled = true;
    public bool Dirty = false;
    public SpriteEffect? Effect = null;
    public ITexture? Texture = null;
    public IShaderResourceBinding? ShaderResourceBinding = null;
    public IPipelineState? PipelineState = null;
    public SpriteBatch? RefBatch = null;
}

public struct SpriteInstanceBatchElement()
{
    public Vector2 Position;
    public Vector2 Scale = new Vector2(1);
    public Vector2 Anchor;
    public float Angle;
    public float Depth;
}

public struct SpriteInstanceBatchItem(IBuffer instancingBuffer)
{
    public readonly object Sync = new();
    public bool Enabled = true;
    public SpriteInstanceBatchElement[] Items = [];
    public Matrix3x3[] Transforms = [];
    public Color Color = Color.Black;
    public bool Dirty = true;
    public bool DirtyInstances = true;
    public ITexture? Texture = null;
    public IShaderResourceBinding? ShaderResourceBinding = null;
    public IPipelineState? PipelineState = null;
    public IBuffer InstanceBuffer = instancingBuffer;
    public SpriteInstanceBatch? RefBatch = null;
}