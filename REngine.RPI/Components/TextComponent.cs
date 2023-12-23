using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;

namespace REngine.RPI.Components
{
	public sealed class TextComponent : BaseSpriteComponent<TextComponent>
	{
		private readonly ITextRenderer pTextRenderer;
		private readonly object pSync = new();

		private TextRendererBatch? pTextBatch;
		private Transform2DSnapshot pLastSnapshot;

		private string pText2Render = string.Empty;
		private string pFontName = string.Empty;

		public string Text { get; set; } = string.Empty;
		public uint TextSize { get; set; } = 16;
		public float HorizontalSpacing { get; set; } = 0;
		public float VerticalSpacing { get; set; } = 0;
		public Color Color { get; set; } = Color.White;

		public RectangleF Bounds => pTextBatch?.Bounds ?? new RectangleF();

		public string FontName
		{
			get => pFontName;
			set
			{
				lock (pFontName)
				{
					if (Equals(pFontName, value))
						return;
					pFontName = value;
					pTextBatch?.Dispose();
					pTextBatch = null;
				}
			}
		}

		public TextComponent(
			IServiceProvider provider,
			ITextRenderer textRenderer) : base(provider)
		{
			pTextRenderer = textRenderer;
		}

		protected override void OnDraw(ISpriteBatch spriteBatch)
		{
			lock (pSync)
			{
				if (string.IsNullOrEmpty(FontName))
					return;

				pTextBatch ??= pTextRenderer.CreateBatch(FontName);
				pTextBatch.Text = pText2Render;
				pTextBatch.Color = Color;
				pTextBatch.Position = pLastSnapshot.WorldPosition;
				pTextBatch.Size = TextSize;
				pTextBatch.HorizontalSpacing = HorizontalSpacing;
				pTextBatch.VerticalSpacing = VerticalSpacing;

				//mSpriteBatch.Draw(pTextBatch);
			}
		}

		protected override void OnBeginRender()
		{
			Transform.GetSnapshot(out pLastSnapshot);
			pText2Render = Text;
		}

		protected override void OnDispose()
		{
			base.OnDispose();

			pTextBatch?.Dispose();
		}
	}
}
