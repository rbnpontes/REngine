using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
    public class TextRendererBatch(
        IDevice device,
        IPipelineState fontPipeline,
        IShaderResourceBinding srb,
        IBuffer constantBuffer,
        Font font)
        : Batch, IDisposable
    {
        private struct BufferData
        {
            public Vector4 Color;
            public Vector4 PositionAndSizes;
        }

        private struct CharData
        {
            public Vector4 Bounds;
            public Vector4 PositionAndAtlasSize;
        }

        private readonly object pSync = new();

        private bool pEnabled = true;
        private bool pIsLocked;
        private bool pDirtyBounds;
        private bool pDirty;

        private IBuffer? pVBuffer;
        private string pText = string.Empty;

        private uint pSize = 16;
        private Vector2 pPosition = Vector2.Zero;
        private float pHorizontalSpacing = 0;
        private float pVerticalSpacing = 0;

        private CharData[] pCharData = Array.Empty<CharData>();

        private RectangleF pBounds = new();
        private RectangleF? pDefinitiveBounds = new();

        private BufferData pCBufferData = new();

        public bool Enabled
        {
            get
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                return pEnabled;
            }
            set
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                pEnabled = value;
            }
        }

        public string Text
        {
            get
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                return pText;
            }
            set
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                pDirtyBounds |= !string.Equals(Text, value, StringComparison.CurrentCulture);
                pText = value;
            }
        }

        public Vector2 Position
        {
            get
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                return pPosition;
            }
            set
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                if (pPosition == value)
                    return;
                pPosition = value;
                pDefinitiveBounds = null;
            }
        }

        public uint Size
        {
            get
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                return pSize;
            }
            set
            {
                lock (pSync)
                {
#if RENGINE_VALIDATIONS
                    ObjectDisposedException.ThrowIf(IsDisposed, this);
                    ValidateLock();
#endif
                    if (pSize == value)
                        return;
                    pSize = value;
                    pDefinitiveBounds = null;
                }
            }
        }

        public float VerticalSpacing
        {
            get
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                return pVerticalSpacing;
            }
            set
            {
                lock (pSync)
                {
#if RENGINE_VALIDATIONS
                    ObjectDisposedException.ThrowIf(IsDisposed, this);
                    ValidateLock();
#endif
                    if (!(Math.Abs(pVerticalSpacing - value) > float.Epsilon)) return;
                    pVerticalSpacing = Math.Max(value, -1.0f);
                    pDefinitiveBounds = null;
                    pDirtyBounds = true;
                }
            }
        }

        public float HorizontalSpacing
        {
            get
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                return pHorizontalSpacing;
            }
            set
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                if (!(Math.Abs(pHorizontalSpacing - value) > float.Epsilon)) return;
                pHorizontalSpacing = Math.Max(value, -1.0f);
                pDefinitiveBounds = null;
                pDirtyBounds = true;
            }
        }

        public Color Color { get; set; } = Color.White;

        public RectangleF Bounds
        {
            get
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                if (pDirtyBounds)
                    UpdateGlyphs();

                // Calculate Final Bounds from Base Bounds
                pDefinitiveBounds ??= pBounds
                    .Scale(1.0f / font.CharSize.Width)
                    .Scale(pSize)
                    .Add(pPosition);

                return pDefinitiveBounds.Value;
            }
        }

        public RectangleF TextBounds
        {
            get
            {
#if RENGINE_VALIDATIONS
                ObjectDisposedException.ThrowIf(IsDisposed, this);
                ValidateLock();
#endif
                return pBounds;
            }
        }

        public bool IsDisposed { get; private set; }

        public override void Render(BatchRenderInfo batchRenderInfo)
        {
            Lock();
            if (pEnabled)
                Draw(batchRenderInfo.CommandBuffer);
            Unlock();
        }

        private void Draw(ICommandBuffer commandBuffer)
        {
            Update(commandBuffer);

            if (string.IsNullOrEmpty(Text) || pVBuffer is null)
                return;

            UpdateCBuffer(commandBuffer);
            commandBuffer
                .SetVertexBuffer(pVBuffer)
                .SetPipeline(fontPipeline)
                .CommitBindings(srb)
                .Draw(new DrawArgs
                {
                    NumVertices = 4,
                    NumInstances = (uint)pText.Length
                });
        }

        private void UpdateCBuffer(ICommandBuffer cmd)
        {
            pCBufferData.Color = new Vector4(
                Color.R / 255.0f,
                Color.G / 255.0f,
                Color.B / 255.0f,
                Color.A / 255.0f
            );
            pCBufferData.PositionAndSizes = new Vector4(
                Position.X,
                Position.Y,
                Size,
                font.CharSize.Width
            );
            var mappedData = cmd.Map<BufferData>(constantBuffer, MapType.Write, MapFlags.Discard);
            mappedData[0] = pCBufferData;
            cmd.Unmap(constantBuffer, MapType.Write);
        }

        private void Update(ICommandBuffer commandBuffer)
        {
            UpdateGlyphs();
            UpdateVBuffer(commandBuffer);
        }

        private void UpdateGlyphs()
        {
            if (!pDirtyBounds)
                return;

            pDirtyBounds = false;
            if (string.IsNullOrEmpty(Text))
                return;

            var chars = pCharData;
            if (pCharData.Length < pText.Length)
                chars = new CharData[pText.Length];

            RectangleF currBounds = new();

            var baseX = 0.0f;
            var baseY = 0.0f;
            var currHeight = 0.0f;
            for (var i = 0; i < pText.Length; ++i)
            {
                if (pText[i] == '\n')
                {
                    baseY += currHeight + (pVerticalSpacing * currHeight);
                    baseX = currHeight = 0.0f;
                    continue;
                }

                var glyphIdx = font.GetGlyphIndex(Text[i]);
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

                chars[i] = new CharData
                {
                    PositionAndAtlasSize = new Vector4(
                        min.X,
                        min.Y,
                        font.AtlasSize.Width,
                        font.AtlasSize.Height
                    ),
                    Bounds = bounds.ToVector4()
                };

                baseX += advance.X + (pHorizontalSpacing * bounds.Width);

                currHeight = Math.Max(currHeight, bounds.Height);
                currBounds = currBounds.Merge(RectangleF.FromLTRB(min.X, min.Y, max.X, max.Y));
            }

            pBounds = currBounds;

            pCharData = chars;
            pDefinitiveBounds = null;
            pDirty = true;
        }

        private void UpdateVBuffer(ICommandBuffer cmd)
        {
            if (!pDirty)
                return;

            pDirty = false;

            if (string.IsNullOrEmpty(pText))
                return;

            var requiredSize = Unsafe.SizeOf<CharData>() * pText.Length;
            if (pVBuffer is null)
            {
                pVBuffer = AllocateVBuffer();
                return;
            }

            if (requiredSize > (int)pVBuffer.Size)
            {
                pVBuffer.Dispose();
                pVBuffer = AllocateVBuffer();
                return;
            }

            ReadOnlySpan<CharData> data = new(pCharData, 0, pText.Length);
            cmd.UpdateBuffer(
                pVBuffer,
                0,
                data
            );
        }

        private IBuffer AllocateVBuffer()
        {
            return device.CreateBuffer(new BufferDesc
            {
                Name = "TextRenderer Instancing Data",
                BindFlags = BindFlags.VertexBuffer,
                Usage = Usage.Default,
                Size = (ulong)(Unsafe.SizeOf<CharData>() * pCharData.Length)
            }, pCharData);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            OnDispose();

            pVBuffer?.Dispose();
            pVBuffer = null;
            GC.SuppressFinalize(this);
        }

        protected virtual void OnDispose()
        {
        }

        public void Lock()
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            Monitor.Enter(pSync);
            pIsLocked = true;
        }

        public void Unlock()
        {
#if RENGINE_VALIDATIONS
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ValidateLock();
#endif
            Monitor.Exit(pSync);
        }

        private void ValidateLock()
        {
            if (!pIsLocked)
                throw new Exception($"{nameof(TextRendererBatch)} must lock first");
        }
    }
}