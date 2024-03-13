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
using REngine.Core.WorldManagement;
using REngine.Game.Components;
using REngine.Sandbox.BaseSample;

namespace REngine.Sandbox.Samples.BasicSamples
{
#if RENGINE_SPRITEBATCH
	[Sample("Text Rendering")]
	internal class TextRendering(
		ISpriteBatch spriteBatch,
		IRenderer renderer,
		IImGuiSystem imGuiSystem,
		IAssetManager assetManager,
		EntityManager entityManager) : ISample
	{
		private readonly object pSync = new object();

		public IWindow? Window { get; set; }
		private IRenderFeature? pSpriteFeature;

		private TextComponent? pComponent;
		private int pTextSize = 1;
		private bool pEnabled = true;
		private float pHorizontalSpacing;
		private float pVerticalSpacing;
		private float pAngle;
		private string pText = "Hello World";
		private Vector4 pColor = new();
		private RectangleF pLastBounds = new();

		public void Dispose()
		{
			renderer.RemoveFeature(pSpriteFeature);
			entityManager.DestroyAll();
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
			var entity = entityManager.CreateEntity(nameof(TextComponent));
			pComponent = entity.CreateComponent<TextComponent>();
			pComponent.SetFont(fontAsset.Font);
			
			pHorizontalSpacing = pComponent.HorizontalSpacing;
			pVerticalSpacing = pComponent.VerticalSpacing;
			pColor = pComponent.Color.ToVector4();

			imGuiSystem = provider.Get<IImGuiSystem>();
			imGuiSystem.OnGui += OnGui;
		}

		public void Update(IServiceProvider provider)
		{
			if (pComponent is null || Window is null)
				return;

			pComponent.Enabled = pEnabled;
			pComponent.HorizontalSpacing = pHorizontalSpacing;
			pComponent.VerticalSpacing = pVerticalSpacing;
			pComponent.Text = pText;
			pComponent.Color = pColor.ToColor();
			pComponent.Transform.Rotation = pAngle;
			pComponent.Transform.Scale = new Vector2(pTextSize / 16.0f);
			pComponent.Transform.Position = new Vector2(
				Window.Size.Width * 0.5f - (pComponent.Bounds.Width * 0.5f),
				Window.Size.Height * 0.5f - (pComponent.Bounds.Height * 0.5f)
			);

			pLastBounds = pComponent.Bounds;
		}

		private bool pDrawTextBounds = false;
		private float pOffset = 0;
		private void OnGui(object? sender, EventArgs e)
		{
			ImGui.Begin("TextRenderer Settings");

			ImGui.Checkbox("Enabled", ref pEnabled);
			ImGui.SliderInt("Font Size", ref pTextSize, 6, 100);
			ImGui.DragFloat("Rotation", ref pAngle, 0.01f);
			ImGui.SliderFloat("Horizontal Spacing", ref pHorizontalSpacing, -10, 10);
			ImGui.SliderFloat("Vertical Spacing", ref pVerticalSpacing, -10, 10);
			ImGui.ColorPicker4("Text Color", ref pColor);
			ImGui.InputTextMultiline("Text", ref pText, 200, new Vector2(200, 13 * 3));
			ImGui.Checkbox("Debug Text Bounds", ref pDrawTextBounds);
			ImGui.DragFloat("Debug Text Bounds Offset", ref pOffset, 0.1f);
			if (pDrawTextBounds)
				DrawBounds(pLastBounds, pOffset);
			
			ImGui.End();
		}

		private static void DrawBounds(RectangleF rect, float offset)
		{
			var color = 0xFF00FF00;
			
			var min = rect.Location.ToVector2();
			var max = new Vector2(rect.Right, rect.Bottom);

			min.Y += offset;
			max.Y += offset;

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
