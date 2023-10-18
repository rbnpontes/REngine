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

		private TextRendererBatch? pBatch;

		public void Dispose()
		{
			pRenderer?.RemoveFeature(pSpriteFeature);
			pSpriteFeature?.Dispose();

			pBatch?.Dispose();
			if (pSpriteBatch != null)
				pSpriteBatch.OnDraw -= OnDraw;
		}

		public void Load(IServiceProvider provider)
		{
			pSpriteBatch = provider.Get<ISpriteBatch>();
			pRenderer = provider.Get<IRenderer>();

			pRenderer.AddFeature(pSpriteFeature = pSpriteBatch.Feature);

			// Load Font
			FontAsset fontAsset = new();
			fontAsset.Name = "Anonymous Pro";
			using (FileStream stream = new(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Fonts/Anonymous Pro.ttf"), FileMode.Open))
				fontAsset.Load(stream).Wait();

			pTextRenderer = provider.Get<ITextRenderer>();
			pBatch = pTextRenderer.CreateBatch(new TextRendererCreateInfo
			{
				Color = Color.Green,
				Font = fontAsset.Font,
				Position = Vector2.Zero,
				Text = "Hello World"
			});


			pSpriteBatch.OnDraw += OnDraw;
		}


		public void Update(IServiceProvider provider)
		{
		}

		private void OnDraw(object? sender, EventArgs e)
		{
			if(pSpriteBatch is null || pBatch is null) return;

			pSpriteBatch.Draw(pBatch);
		}
	}
#endif
}
