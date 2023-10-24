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

namespace REngine.Sandbox.Samples
{
#if RENGINE_SPRITEBATCH
	[Sample("Text Rendering")]
	internal class TextRendering : ISample
	{
		private readonly object pSync = new object();

		public IWindow? Window { get; set; }
		private ISpriteBatch? pSpriteBatch;
		private IRenderFeature? pSpriteFeature;
		private IRenderer? pRenderer;
		private ITextRenderer? pTextRenderer;
		private IImGuiSystem? pImGuiSystem;

		private TextRendererBatch? pBatch;

		private int pTextSize = 0;
		private float pHorizontalSpacing = 0;
		private float pVerticalSpacing = 0;
		private string pText = string.Empty;
		private RectangleF pLastBounds = new();

		public void Dispose()
		{
			pRenderer?.RemoveFeature(pSpriteFeature);
			pSpriteFeature?.Dispose();

			pBatch?.Dispose();
			if (pSpriteBatch != null)
				pSpriteBatch.OnDraw -= OnDraw;

			if(pImGuiSystem != null)
				pImGuiSystem.OnGui -= OnGui; ;
		}

		public void Load(IServiceProvider provider)
		{
			if (Window is null)
				return;
			pSpriteBatch = provider.Get<ISpriteBatch>();
			pRenderer = provider.Get<IRenderer>();

			pRenderer.AddFeature(pSpriteFeature = pSpriteBatch.Feature);

			// Load Font
			FontAsset fontAsset = new();
			fontAsset.Name = "Anonymous Pro";
			using (FileStream stream = new(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Fonts/Anonymous Pro.ttf"), FileMode.Open))
				fontAsset.Load(stream).Wait();

			pTextRenderer = provider.Get<ITextRenderer>();
			pBatch = pTextRenderer.SetFont(fontAsset.Font).CreateBatch(fontAsset.Font.Name);

			pBatch.Text = pText = "Hello World";
			pTextSize = (int)pBatch.Size;
			pHorizontalSpacing = pBatch.HorizontalSpacing;
			pVerticalSpacing = pBatch.VerticalSpacing;

			pSpriteBatch.OnDraw += OnDraw;

			pImGuiSystem = provider.Get<IImGuiSystem>();
			pImGuiSystem.OnGui += OnGui;
		}

		public void Update(IServiceProvider provider)
		{
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
				ImGui.InputTextMultiline("Text", ref pText, 200, new Vector2(200, 13 * 3));
				ImGui.Checkbox("Debug Text Bounds", ref pDrawTextBounds);

				if (pDrawTextBounds)
					DrawBounds(pBatch.Bounds);
			}

			ImGui.End();
		}

		private float pOffset = 0f;
		private void DrawBounds(RectangleF rect)
		{
			uint color = 0xFF00FF00;
			
			Vector2 min = rect.Location.ToVector2();
			Vector2 max = new Vector2(rect.Right, rect.Bottom);

			ImGui.DragFloat("Offset", ref pOffset);

			min.Y += pOffset;
			max.Y += pOffset;

			var drawList = ImGui.GetBackgroundDrawList();
			drawList.AddText(min, color, min.ToString());
			drawList.AddText(max, color, max.ToString());
			drawList.AddRect(
				new Vector2(min.X, min.Y),
				new Vector2(max.X, max.Y),
				color
			);
		}

		private void OnDraw(object? sender, EventArgs e)
		{
			if(pSpriteBatch is null || pBatch is null || Window is null) return;

			// ImGui can run in our thread, we must lock this values before read
			lock (pSync)
			{
				// Any slighty changes to this properties will force
				// bound box changes, in this case we must 
				// be carefully while is reading bounding box
				// because this property will trigger text calculation
				pBatch.Size = (uint)pTextSize;
				pBatch.HorizontalSpacing = pHorizontalSpacing;
				pBatch.VerticalSpacing = pVerticalSpacing;
				pBatch.Text = pText;
			}

			//var bounds = pBatch.Bounds;
			//Vector2 position = new Vector2(
			//	(Window.Size.Width * 0.5f) - (bounds.Width * 0.5f),
			//	(Window.Size.Height * 0.5f) - (bounds.Height * 0.5f)
			//);
			pBatch.Position = new Vector2(
				Window.Size.Width * 0.5f,
				Window.Size.Height * 0.5f
			);

			lock (pSync)
				pLastBounds = pBatch.Bounds;

			pSpriteBatch.Draw(pBatch);
		}
	}
#endif
}
