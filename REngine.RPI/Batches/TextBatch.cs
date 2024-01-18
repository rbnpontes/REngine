using System.Buffers;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.Core.Reflection;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RPI.Constants;
using REngine.RPI.Effects;
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace REngine.RPI.Batches;

public struct TextBatchDesc()
{
    public bool Enabled = true;
    public string Text = string.Empty;
    public int ZIndex;
    public float VerticalSpacing;
    public float HorizontalSpacing;
    public Color Color = Color.White;
    public Matrix4x4 Transform = Matrix4x4.Identity;
    public TextEffect? Effect;
}

public struct TextBatchBasicUpdateDesc()
{
    public bool Enabled = true;
    public int ZIndex;
    public Matrix4x4 Transform = Matrix4x4.Identity;
    public Color Color;
    public TextEffect? Effect;
}

public abstract class TextBatch(
    ISpriteBatch spriteBatch,
    IBufferManager bufferManager,
    IShaderResourceBindingCache srbCache,
    Font font,
    ITexture texture
) : Batch
{
    protected struct BufferData
    {
        public Matrix4x4 Transform;
        public Vector4 Color;
        public Vector2 FontOffset;
    }
    protected struct CharData
    {
        public Vector4 Bounds;
        public Vector4 PositionAndAtlasSize;
    }

    private readonly object pSync = new();
    
    protected bool mEnabled = true;
    protected int mZIndex;

    protected BufferData mBufferData;
    protected CharData[] mCharData = [];
    protected uint mInstanceCount;
    protected TextEffect? mEffect;

    private IPipelineState? pPipelineState;
    private IShaderResourceBinding? pShaderResourceBinding;
    public RectangleF Bounds { get; protected set; }

    protected override void OnDispose()
    {
        spriteBatch.RemoveBatch(this);
        DisposableQueue.Enqueue(pShaderResourceBinding);
    }

    public override int GetSortIndex() => mZIndex;
    public virtual void Update(TextBatchDesc desc)
    {
        var charData = CreateGlyphs(desc, out var bounds);
        var bufferData = new BufferData()
        {
            Color = desc.Color.ToVector4(),
            Transform = desc.Transform,
            FontOffset = bounds.Location.ToVector2()
        };

        lock (pSync)
        {
            mEnabled = desc.Enabled;
            if (charData.Length == 0)
                mEnabled = false;
            mZIndex = desc.ZIndex;
            mEffect = desc.Effect;
            mInstanceCount = (uint)desc.Text.Length;
            mBufferData = bufferData;
            mCharData = charData;
            Bounds = bounds;
        }
    }
    public virtual void Update(TextBatchBasicUpdateDesc desc)
    {
        lock (pSync)
        {
            mEnabled = desc.Enabled;
            mBufferData = new BufferData()
            {
                Color = desc.Color.ToVector4(),
                Transform = desc.Transform,
                FontOffset = mBufferData.FontOffset
            };
            mZIndex = desc.ZIndex;
            if (mEffect == desc.Effect) 
                return;
            mEffect = desc.Effect;
            pPipelineState = null;
            DisposableQueue.Enqueue(pShaderResourceBinding);
        }
    }
    protected virtual CharData[] CreateGlyphs(TextBatchDesc desc, out RectangleF outputBounds)
    {
        if (string.IsNullOrEmpty(desc.Text))
        {
            outputBounds = new RectangleF();
            return [];
        }

        var text = desc.Text;
        var charData = mCharData;
        if (text.Length != mCharData.Length)
            charData = new CharData[desc.Text.Length * 2/*Allocate twice as required*/];

        RectangleF currBounds = new();
        var baseX = 0.0f;
        var baseY = 0.0f;
        var currHeight = 0.0f;
        for (var i = 0; i < text.Length; ++i)
        {
            if (text[i] == '\n')
            {
                baseY += currHeight + (desc.VerticalSpacing * currHeight);
                baseX = currHeight = 0.0f;
                continue;
            }

            var glyphIdx = font.GetGlyphIndex(text[i]);
            var bounds = font.GetBounds(glyphIdx);
            var offset = font.GetOffset(glyphIdx);
            var advance = font.GetAdvance(glyphIdx);

            var min = new Vector2(
                baseX + offset.X,
                baseY + (font.CharSize.Height - offset.Y)
            );
            var max = new Vector2(
                min.X + bounds.Width,
                min.Y + bounds.Height
            );

            charData[i] = new CharData()
            {
                PositionAndAtlasSize = new Vector4(
                    min.X,
                    min.Y,
                    font.AtlasSize.Width,
                    font.AtlasSize.Height
                ),
                Bounds = bounds.ToVector4()
            };

            baseX += advance.X + (desc.HorizontalSpacing * bounds.Width);

            currHeight = Math.Max(currHeight, bounds.Height);
            currBounds = currBounds.Merge(RectangleF.FromLTRB(min.X, min.Y, max.X, max.Y));
        }

        outputBounds = currBounds;
        return charData;
    }

    public override void Render(BatchRenderInfo batchRenderInfo)
    {
        BufferData bufferData;
        TextEffect effect;
        IBuffer instancingBuffer;
        uint instanceCount;

        lock (pSync)
        {
            if (!mEnabled)
                return;
            bufferData = mBufferData;
            effect = mEffect ?? spriteBatch.DefaultTextEffect;
            instancingBuffer = UpdateBuffer(batchRenderInfo);
            instanceCount = mInstanceCount;
        }
        
        var dirtySrb = false;
        if (pPipelineState is null)
        {
            dirtySrb = true;
            pPipelineState = effect.BuildPipeline();
            pShaderResourceBinding = null;
        }
        else if (pPipelineState != effect.BuildPipeline())
        {
            dirtySrb = true;
            pPipelineState = effect.BuildPipeline();
        }

        pPipelineState ??= effect.BuildPipeline();

        if (pShaderResourceBinding is null)
            dirtySrb = true;

        if (dirtySrb)
        {
            DisposableQueue.Enqueue(pShaderResourceBinding);
            var resMapping = new ResourceMapping();
            resMapping
                .Add(ShaderTypeFlags.Vertex, ConstantBufferNames.Frame, bufferManager.GetBuffer(BufferGroupType.Frame))
                .Add(ShaderTypeFlags.Vertex, ConstantBufferNames.Object,
                    bufferManager.GetBuffer(BufferGroupType.Object))
                .Add(ShaderTypeFlags.Pixel, TextureNames.MainTexture,
                    texture.GetDefaultView(TextureViewType.ShaderResource));
            pShaderResourceBinding = srbCache.Build(pPipelineState, resMapping);
        }

        var command = batchRenderInfo.CommandBuffer;

        {
            // Update Constant Buffer
            var cbuffer = bufferManager.GetBuffer(BufferGroupType.Object);
            var mappedData = command.Map<BufferData>(cbuffer, MapType.Write, MapFlags.Discard);
            mappedData[0] = bufferData;
            command.Unmap(cbuffer, MapType.Write);
        }

        command
            .SetVertexBuffer(instancingBuffer)
            .SetPipeline(pPipelineState)
            .CommitBindings(pShaderResourceBinding)
            .Draw(new DrawArgs()
            {
                NumVertices = 4,
                NumInstances = instanceCount
            });
    }

    protected abstract IBuffer UpdateBuffer(BatchRenderInfo batchRenderInfo);
}

public sealed class DefaultTextBatch(
    ISpriteBatch spriteBatch,
    IBufferManager bufferManager,
    IShaderResourceBindingCache srbCache,
    Font font,
    ITexture texture) : TextBatch(
    spriteBatch,
    bufferManager,
    srbCache,
    font,
    texture)
{
    private readonly object pSync = new();
    private ulong pRequiredInstanceSize;
    private bool pDirtyBuffer;
    private IBuffer? pInstancingBuffer;

    protected override void OnDispose()
    {
        base.OnDispose();
        DisposableQueue.Enqueue(pInstancingBuffer);
    }

    public override void Update(TextBatchDesc desc)
    {
        lock (pSync)
        {
            base.Update(desc);
            pRequiredInstanceSize = (ulong)(Unsafe.SizeOf<CharData>() * mCharData.Length);
            pDirtyBuffer = true;
        }
    }

    protected override IBuffer UpdateBuffer(BatchRenderInfo batchRenderInfo)
    {
        bool isDirty;
        ulong requiredInstanceSize;
        int instanceCount;
        lock (pSync)
        {
            isDirty = pDirtyBuffer;
            pDirtyBuffer = false;
            instanceCount = (int)mInstanceCount;
            requiredInstanceSize = pRequiredInstanceSize;
        }

        if (pInstancingBuffer is null)
        {
            pInstancingBuffer = AllocateBuffer(requiredInstanceSize * 2);
            isDirty = false;
        } 
        else if (requiredInstanceSize > pInstancingBuffer.Size)
        {
            DisposableQueue.Enqueue(pInstancingBuffer);
            pInstancingBuffer = AllocateBuffer(requiredInstanceSize * 2);
            isDirty = false;
        }

        if (!isDirty)
            return pInstancingBuffer;

        var command = batchRenderInfo.CommandBuffer;
        // Update only allocated data
        command.UpdateBuffer(pInstancingBuffer, 0, mCharData.AsSpan(0, instanceCount));
        return pInstancingBuffer;
    }

    private IBuffer AllocateBuffer(ulong bufferSize)
    {
        return bufferManager.Allocate(new BufferDesc()
        {
            Name = "[Default]Text Instancing Buffer",
            BindFlags = BindFlags.VertexBuffer,
            Size = bufferSize,
            Usage = Usage.Default
        }, mCharData);
    }
    
    public static DefaultTextBatch Build(
        Font font,
        ITexture fontAtlas,
        IServiceProvider serviceProvider
    )
    {
        var spriteBatch = serviceProvider.Get<ISpriteBatch>();
        var bufferManager = serviceProvider.Get<IBufferManager>();
        var srbCache = serviceProvider.Get<IShaderResourceBindingCache>();
        return new DefaultTextBatch(
            spriteBatch,
            bufferManager,
            srbCache,
            font,
            fontAtlas
        );
    }
}

public sealed class DynamicTextBatch(
    ISpriteBatch spriteBatch, 
    IBufferManager bufferManager, 
    IShaderResourceBindingCache srbCache, 
    Font font, 
    ITexture texture) : TextBatch(
    spriteBatch, 
    bufferManager, 
    srbCache, 
    font, 
    texture)
{
    private static RefCount<IBuffer> sGlobalTextInstanceBuffer = RefCount<IBuffer>.Empty;
    private IBuffer? pInstanceBuffer;
    protected override void OnDispose()
    {
        base.OnDispose();
        DisposableQueue.Enqueue(sGlobalTextInstanceBuffer);
    }
    protected override unsafe IBuffer UpdateBuffer(BatchRenderInfo batchRenderInfo)
    {
        var requiredSize = (ulong)Unsafe.SizeOf<CharData>() * mInstanceCount;

        if (sGlobalTextInstanceBuffer.Count == 0)
        {
            sGlobalTextInstanceBuffer = new RefCount<IBuffer>(AllocateBuffer(requiredSize * 2));
            pInstanceBuffer = sGlobalTextInstanceBuffer.Ref;
        }
        else if (sGlobalTextInstanceBuffer.Ref.Size < requiredSize)
        {
            DisposableQueue.Enqueue(sGlobalTextInstanceBuffer.Ref);
            sGlobalTextInstanceBuffer = new RefCount<IBuffer>(AllocateBuffer(requiredSize * 2));
            pInstanceBuffer = sGlobalTextInstanceBuffer.Ref;
        }

        if (pInstanceBuffer is null || pInstanceBuffer != sGlobalTextInstanceBuffer.Ref)
        {
            pInstanceBuffer = sGlobalTextInstanceBuffer.Ref;
            sGlobalTextInstanceBuffer.AddRef();
        }

        var command = batchRenderInfo.CommandBuffer;
        var gpuPtr = command.Map(pInstanceBuffer, MapType.Write, MapFlags.Discard);
        fixed (void* dataPtr = mCharData.AsSpan(0, (int)mInstanceCount))
            Buffer.MemoryCopy(dataPtr, gpuPtr.ToPointer(), requiredSize, requiredSize);
        command.Unmap(pInstanceBuffer, MapType.Write);
        
        return pInstanceBuffer;
    }
    private IBuffer AllocateBuffer(ulong bufferSize)
    {
        return bufferManager.GetInstancingBuffer(bufferSize, true);
    }

    public static DynamicTextBatch Build(
        Font font,
        ITexture fontAtlas,
        IServiceProvider serviceProvider
    )
    {
        var spriteBatch = serviceProvider.Get<ISpriteBatch>();
        var bufferManager = serviceProvider.Get<IBufferManager>();
        var srbCache = serviceProvider.Get<IShaderResourceBindingCache>();
        return new DynamicTextBatch(
            spriteBatch,
            bufferManager,
            srbCache,
            font,
            fontAtlas
        );
    }
}