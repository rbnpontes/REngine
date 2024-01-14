using System.Buffers;
using System.Drawing;
using System.Numerics;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.RHI;

namespace REngine.RPI.Batches;

public struct TextBatchDesc()
{
    public bool Enabled = true;
    public string Text = string.Empty;
    public int ZIndex;
    public uint FontSize;
    public float VerticalSpacing;
    public float HorizontalSpacing;
    public Color Color = Color.White;
    public Matrix4x4 Transform = Matrix4x4.Identity;
}

public abstract class TextBatch(
    IBufferManager bufferManager,
    Font font,
    ITexture texture
) : Batch
{
    protected struct BufferData
    {
        public Matrix4x4 Transform;
        public Vector4 Color;
    }
    protected struct CharData
    {
        public Vector4 Bounds;
        public Vector4 PositionAndAtlasSize;
    }

    protected readonly object mSync = new();
    protected bool mEnabled = true;
    protected int mZIndex;
    
    protected BufferData mBufferData;
    protected CharData[] mCharData = [];

    private IBuffer? pInstanceBuffer;
    public RectangleF Bounds { get; protected set; }

    public override int GetSortIndex() => mZIndex;

    public virtual void Update(TextBatchDesc desc)
    {
        var charData = CreateGlyphs(desc, out var bounds);
        var bufferData = new BufferData()
        {
            Color = desc.Color.ToVector4(),
            Transform = desc.Transform
        };
        
        lock (mSync)
        {
            mEnabled = desc.Enabled;
            mZIndex = desc.ZIndex;
            mBufferData = bufferData;
            mCharData = charData;
            Bounds = bounds;
        }
    }
    protected virtual CharData[] CreateGlyphs(TextBatchDesc desc, out RectangleF outputBounds)
    {
        if (string.IsNullOrEmpty(desc.Text))
        {
            outputBounds = new RectangleF();
            return [];
        }

        var chardData = mCharData;
        if (desc.Text.Length != mCharData.Length)
            chardData = new CharData[desc.Text.Length];

        RectangleF currBounds = new();
        var text = desc.Text;
        var baseX = 0.0f;
        var baseY = 0.0f;
        var currHeight = 0.0f;
        for (var i = 0; i < mCharData.Length; ++i)
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

            mCharData[i] = new CharData()
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
        return chardData;
    }
    public override void Render(BatchRenderInfo batchRenderInfo)
    {
        bool enabled;
        BufferData bufferData;
        lock (mSync)
        {
            enabled = mEnabled;
            bufferData = mBufferData;

            UpdateBuffer(batchRenderInfo);
        }
    }

    protected abstract void UpdateBuffer(BatchRenderInfo batchRenderInfo);
}