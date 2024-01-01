using ImGuiNET;
using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;
using REngine.Sandbox.BaseSample;

namespace REngine.Sandbox.Samples.BasicSamples
{
#if RENGINE_SPRITEBATCH
	[Sample("Text Rendering")]
	internal class TextRendering(
		ISpriteBatch spriteBatch,
		IRenderer renderer,
		IImGuiSystem imGuiSystem,
		IAssetManager assetManager) : ISample
	{
		private readonly object pSync = new object();

		public IWindow? Window { get; set; }
		private IRenderFeature? pSpriteFeature;

		private TextRendererBatch? pBatch;

		private int pTextSize = 0;
		private bool pEnabled = true;
		private float pHorizontalSpacing = 0;
		private float pVerticalSpacing = 0;
		private string pText = string.Empty;
		private Vector4 pColor = new();
		private RectangleF pLastBounds = new();

		public void Dispose()
		{
			renderer.RemoveFeature(pSpriteFeature);
			pSpriteFeature?.Dispose();

			pBatch?.Dispose();
			
			//spriteBatch.OnDraw -= OnDraw;
			imGuiSystem.OnGui -= OnGui;
		}

		public void Load(IServiceProvider provider)
		{
			if (Window is null)
				return;

			pSpriteFeature = spriteBatch.CreateRenderFeature();
			renderer.AddFeature(pSpriteFeature);

			// Load Font
			var fontAsset = assetManager.GetAsset<FontAsset>("Fonts/Anonymous-Pro.ttf");
			pBatch = spriteBatch.CreateText(new TextCreateInfo(fontAsset.Font)
			{
				Text = pText = "Hello World",
				FontSize = 16
			});
			pTextSize = 16;
			
			pHorizontalSpacing = pBatch.HorizontalSpacing;
			pVerticalSpacing = pBatch.VerticalSpacing;

			pColor = pBatch.Color.ToVector4();

			imGuiSystem = provider.Get<IImGuiSystem>();
			imGuiSystem.OnGui += OnGui;
		}

		public void Update(IServiceProvider provider)
		{
			if (pBatch is null || Window is null)
				return;
			
			pBatch.Lock();
			var bounds = pBatch.TextBounds;
			pBatch.Enabled = pEnabled;
			pBatch.Size = (uint)pTextSize;
			pBatch.HorizontalSpacing = pHorizontalSpacing;
			pBatch.VerticalSpacing = pVerticalSpacing;
			pBatch.Text = pText;
			pBatch.Color = pColor.ToColor();
			pBatch.Position = new Vector2(
				(Window.Size.Width * 0.5f) - (bounds.Width * 0.5f),
				(Window.Size.Height * 0.5f) - (bounds.Height * 0.5f)
			);
			pLastBounds = pBatch.Bounds;
			pBatch.Unlock();
		}

		private bool pDrawTextBounds = false;
		private void OnGui(object? sender, EventArgs e)
		{
			if (pBatch is null)
				return;

			ImGui.Begin("TextRenderer Settings");

			// Usually, ImGui runs in your own thread
			// In this case to prevent unexpected issues
			// we will lock our thread before change values
			lock (pSync)
			{
				ImGui.SliderInt("Font Size", ref pTextSize, 6, 100);
				ImGui.SliderFloat("Horizontal Spacing", ref pHorizontalSpacing, -10, 10);
				ImGui.SliderFloat("Vertical Spacing", ref pVerticalSpacing, -10, 10);
				ImGui.ColorPicker4("Text Color", ref pColor);
				ImGui.InputTextMultiline("Text", ref pText, 200, new Vector2(200, 13 * 3));
				ImGui.Checkbox("Debug Text Bounds", ref pDrawTextBounds);

				if (pDrawTextBounds)
					DrawBounds(pLastBounds);
			}

			ImGui.End();
		}

		private static void DrawBounds(RectangleF rect)
		{
			var color = 0xFF00FF00;
			
			var min = rect.Location.ToVector2();
			var max = new Vector2(rect.Right, rect.Bottom);

			var drawList = ImGui.GetBackgroundDrawList();
			drawList.AddText(min, color, min.ToString());
			drawList.AddText(max, color, max.ToString());
			drawList.AddRect(
				new Vector2(min.X, min.Y),
				new Vector2(max.X, max.Y),
				color
			);
		}
	}
#endif
}
