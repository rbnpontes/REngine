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

		private IBuffer? pVBuffer;
		private string pText = string.Empty;
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
					pDirty = string.Equals(Text, value, StringComparison.CurrentCulture);
					pText = value;
				}
			}
		}

		public Vector2 Position { get; set; } = Vector2.Zero;
		public uint Size { get; set; } = 16;

		public Color Color { get; set; } = Color.White;

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

			UpdateCBuffer(commandBuffer);

			lock (pSync)
			{
				commandBuffer
					.SetVertexBuffer(pVBuffer)
					.SetPipeline(pFontPipeline)
					.CommitBindings(pSRB)
					.Draw(new DrawArgs
					{
						NumVertices = 4,
						NumInstances = (uint)pText.Count()
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
				UpdateGlyphs(commandBuffer);
			return this;
		}

		private void UpdateGlyphs(ICommandBuffer cmd)
		{
			AssertDispose();
			if (pVBuffer is null)
				pDirty = true;

			if (!pDirty)
				return;

			pDirty = false;
			if (string.IsNullOrEmpty(Text))
				return;

			CharData[] chars = new CharData[Text.Length];
			float baseX = 0.0f;

			for (int i = 0; i < chars.Length; ++i)
			{
				byte glyphIdx = pFont.GetGlyhIndex(Text[i]);
				var bounds = pFont.GetBounds(glyphIdx);
				var offset = pFont.GetOffset(glyphIdx);
				var advance = pFont.GetAdvance(glyphIdx);

				chars[i] = new CharData
				{
					PositionAndAtlasSize = new Vector4(
						baseX + offset.X,
						pFont.CharSize.Height - offset.Y,
						pFont.Atlas.Size.Width,
						pFont.Atlas.Size.Height
					),
					Bounds = bounds.ToVector4()
				};

				baseX += advance.X;
			}

			int requiredSize = Unsafe.SizeOf<CharData>() * chars.Length * 2;
			if (pVBuffer is null)
			{
				pVBuffer = AllocateVBuffer(chars);
				return;
			}

			if (requiredSize > (int)pVBuffer.Size)
			{
				pVBuffer.Dispose();
				pVBuffer = AllocateVBuffer(chars);
				return;
			}

			ReadOnlySpan<CharData> data = new(chars);
			cmd.UpdateBuffer(
				pVBuffer,
				0,
				data
			);
		}
		
		private IBuffer AllocateVBuffer(CharData[] data)
		{
			// Allocate Twice as required
			CharData[] copyData = new CharData[data.Length * 2];
			Array.Copy(data, copyData, data.Length);
			return pDevice.CreateBuffer(new BufferDesc
			{
				Name = "TextRenderer Instancing Data",
				BindFlags = BindFlags.VertexBuffer,
				Usage = Usage.Default,
				Size = (ulong)(Unsafe.SizeOf<CharData>() * copyData.Length)
			}, copyData);
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
