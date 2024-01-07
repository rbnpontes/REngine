using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using REngine.Core.Mathematics;
using REngine.RHI;
using SpriteInstanceData = System.Numerics.Matrix4x4;
namespace REngine.RPI.Structs;

public enum SpriteBufferType
{
    Default,
    Dynamic,
    External
}
public struct SpriteBatchItem(SpriteEffect effect)
{
    public readonly object Sync = new();
    public Vector3 Position = Vector3.Zero;
    public Vector2 Anchor = Vector2.Zero;
    public Vector2 Size = Vector2.One;
    public Color Color = Color.Black;
    public float Angle = 0f;
    public bool Enabled = true;
    public Batch? Batch;
    
    public SpriteEffect Effect = effect;
    public Sprite? RefSprite = null;
}

public struct SpriteInstanceBatchElement()
{
    public Vector2 Position;
    public Vector2 Scale = new Vector2(1);
    public Vector2 Anchor;
    public float Angle;
    public float Depth;
}

public struct SpriteInstanceBatchItem(IBuffer instancingBuffer, InstancedSpriteEffect effect)
{
    public readonly object Sync = new();
    public bool Enabled = true;
    public SpriteInstanceBatchElement[] Items = [];
    public SpriteInstanceData[] Transforms = [];
    public Color Color = Color.Black;
    public bool DirtyInstances = true;
    public SpriteBufferType BufferType;
    public Batch? Batch;
    public InstancedSpriteEffect Effect = effect;
    public IBuffer InstanceBuffer = instancingBuffer;
    public InstancedSprite? RefSprite = null;
}