using ImGuiNET;
using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
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
		public IWindow? Window { get; set; }
		private ISpriteBatch? pSpriteBatch;
		private IRenderFeature? pSpriteFeature;
		private IRenderer? pRenderer;
		private ITextRenderer? pTextRenderer;
		private IImGuiSystem? pImGuiSystem;

		private TextRendererBatch? pBatch;

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

			pBatch.Text = "Hello World";
			pBatch.Position = new Vector2(Window.Size.Width / 2.0f, Window.Size.Height / 2.0f);
			pSpriteBatch.OnDraw += OnDraw;

			pImGuiSystem = provider.Get<IImGuiSystem>();
			pImGuiSystem.OnGui += OnGui;
		}

		public void Update(IServiceProvider provider)
		{
		}

		private void OnGui(object? sender, EventArgs e)
		{
			if (pBatch is null)
				return;

			ImGui.Begin("TextRenderer Settings");

			int fontSize = (int)pBatch.Size;
			ImGui.SliderInt("Font Size", ref fontSize, 6, 100);
			pBatch.Size = (uint)fontSize;

			string text = pBatch.Text;
			ImGui.InputText("Text", ref text, 200);
			pBatch.Text = text;

			ImGui.End();
		}

		private void OnDraw(object? sender, EventArgs e)
		{
			if(pSpriteBatch is null || pBatch is null) return;

			pSpriteBatch.Draw(pBatch);
		}
	}
#endif
}
