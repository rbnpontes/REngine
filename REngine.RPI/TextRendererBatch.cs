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
	public class TextRendererBatch : IDisposable
	{
		struct BufferData
		{
			public Vector4 Color;
			public Vector4 PositionAndSizes;
		}

		struct CharData
		{
			public Vector4 Bounds;
			public Vector4 PositionAndAtlasSize;
		}

		private readonly IDevice pDevice;
		private readonly IPipelineState pFontPipeline;
		private readonly IShaderResourceBinding pSRB;
		private readonly IBuffer pCBuffer;
		private readonly Font pFont;
		private readonly object pSync = new();

		private bool pDisposed;
		private bool pDirty;
		private bool pDirtyBounds;

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

		public string Text
		{
			get
			{
				var text = string.Empty;
				lock(pSync)
					text = pText;
				return text;
			}
			set
			{
				lock (pSync)
				{
					if (!string.Equals(Text, value, StringComparison.CurrentCulture))
						pDirtyBounds = true;
					pText = value;
				}
			}
		}

		public Vector2 Position
		{
			get
			{
				Vector2 position;
				lock (pSync)
					position = pPosition;
				return position;
			}
			set
			{
				lock (pSync)
				{
					if(pPosition != value)
					{
						pPosition = value;
						pDefinitiveBounds = null;
					}
				}
			}
		}
		public uint Size 
		{
			get
			{
				uint result;
				lock(pSync)
					result = pSize;
				return result;
			}
			set
			{
				lock (pSync)
				{
					if(pSize != value)
					{
						pSize = value;
						pDefinitiveBounds = null;
					}
				}
			}
		}

		public float VerticalSpacing
		{
			get
			{
				float result;
				lock(pSync)
					result = pVerticalSpacing;
				return result;
			}
			set
			{
				lock (pSync)
				{
					if(pVerticalSpacing != value)
					{
						pVerticalSpacing = Math.Max(value, -1.0f);
						pDefinitiveBounds = null;
						pDirtyBounds = true;
					}
				}
			}
		}
		public float HorizontalSpacing
		{
			get 
			{
				float result;
				lock (pSync)
					result = pHorizontalSpacing;
				return result;
			}
			set
			{
				lock (pSync)
				{
					if(pHorizontalSpacing != value)
					{
						pHorizontalSpacing = Math.Max(value, -1.0f);
						pDefinitiveBounds = null;
						pDirtyBounds = true;
					}
				}
			}
		}
		public Color Color { get; set; } = Color.White;

		public RectangleF Bounds 
		{ 
			get
			{
				RectangleF result;
				lock (pSync)
				{
					if (pDirtyBounds)
						UpdateGlyphs();

					// Calculate Final Bounds from Base Bounds
					if(pDefinitiveBounds is null)
					{
						// glyph bounds is relative to atlas size
						// in this case, we must downscale to font size ratio
						// and scale again by the current size
						pDefinitiveBounds = pBounds
							.Scale(1.0f / pFont.CharSize.Width)
							.Scale(pSize)
							.Add(pPosition);
					}

					result = pDefinitiveBounds.Value;
				}

				return result;
			}
		}

		public RectangleF TextBounds
		{
			get
			{
				RectangleF result;
				lock (pSync)
					result = pBounds;
				return result;
			}
		}

		public bool IsDirty { get => pDirty; }

		public bool IsDisposed { get => pDisposed; }

		public TextRendererBatch(
			IDevice device, 
			IPipelineState fontPipeline,
			IShaderResourceBinding srb,
			IBuffer cbuffer,
			Font font)
		{
			pFontPipeline = fontPipeline;
			pSRB = srb;
			pCBuffer = cbuffer;
			
			pDevice = device;
			pFont = font;
		}

		public TextRendererBatch Draw(ICommandBuffer commandBuffer)
		{
			AssertDispose();
			Update(commandBuffer);

			if (string.IsNullOrEmpty(Text) || pVBuffer is null)
				return this;


			lock (pSync)
			{
				UpdateCBuffer(commandBuffer);
				commandBuffer
					.SetVertexBuffer(pVBuffer)
					.SetPipeline(pFontPipeline)
					.CommitBindings(pSRB)
					.Draw(new DrawArgs
					{
						NumVertices = 4,
						NumInstances = (uint)pText.Length
					});
			}
			return this;
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
				pFont.CharSize.Width
			);
			var mappedData = cmd.Map<BufferData>(pCBuffer, MapType.Write, MapFlags.Discard);
			mappedData[0] = pCBufferData;
			cmd.Unmap(pCBuffer, MapType.Write);
		}

		public TextRendererBatch Update(ICommandBuffer commandBuffer)
		{
			AssertDispose();
			lock (pSync)
			{
				UpdateGlyphs();
				UpdateVBuffer(commandBuffer);
			}
			return this;
		}

		private void UpdateGlyphs()
		{
			AssertDispose();

			if (!pDirtyBounds)
				return;

			pDirtyBounds = false;
			if (string.IsNullOrEmpty(Text))
				return;

			CharData[] chars = pCharData;
			if(pCharData.Length < pText.Length)
				chars = new CharData[pText.Length];

			RectangleF currBounds = new();

			float baseX = 0.0f;
			float baseY = 0.0f;
			float currHeight = 0.0f;
			for (int i = 0; i < pText.Length; ++i)
			{
				if (pText[i] == '\n')
				{
					baseY += currHeight + (pVerticalSpacing * currHeight);
					baseX = currHeight = 0.0f;
					continue;
				}

				byte glyphIdx = pFont.GetGlyhIndex(Text[i]);
				var bounds = pFont.GetBounds(glyphIdx);
				var offset = pFont.GetOffset(glyphIdx);
				var advance = pFont.GetAdvance(glyphIdx);

				Vector2 min = new Vector2(
					baseX + offset.X, 
					baseY + (pFont.CharSize.Height - offset.Y)
				);
				Vector2 max = new Vector2(
					min.X + bounds.Width,
					min.Y + bounds.Height
				);

				chars[i] = new CharData
				{
					PositionAndAtlasSize = new Vector4(
						min.X,
						min.Y,
						pFont.AtlasSize.Width,
						pFont.AtlasSize.Height
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

			int requiredSize = Unsafe.SizeOf<CharData>() * pText.Length;
			if(pVBuffer is null)
			{
				pVBuffer = AllocateVBuffer();
				return;
			}

			if(requiredSize > (int)pVBuffer.Size)
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
			return pDevice.CreateBuffer(new BufferDesc
			{
				Name = "TextRenderer Instancing Data",
				BindFlags = BindFlags.VertexBuffer,
				Usage = Usage.Default,
				Size = (ulong)(Unsafe.SizeOf<CharData>() * pCharData.Length)
			}, pCharData);
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			OnDispose();

			pVBuffer?.Dispose();

			pVBuffer = null;
			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		protected void AssertDispose()
		{
			if (pDisposed)
				throw new ObjectDisposedException("Text Renderer Batch has been disposed");
		}

		protected virtual void OnDispose() { }
	}
}
