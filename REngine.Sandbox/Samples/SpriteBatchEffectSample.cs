using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.RPI;

namespace REngine.Sandbox.Samples
{
	[Sample("SpriteBatch - Effect")]
	internal class SpriteBatchEffectSample : ISample
	{
		private readonly BasicSpriteEffect pSpriteEffect = new("Sample Effect");

		private IRenderFeature? pSpriteFeature;
		private ISpriteBatch? pSpriteBatch;
		private IRenderer? pRenderer;
		public IWindow? Window { get; set; }
		public void Dispose()
		{
			pSpriteEffect.Dispose();

			pRenderer?.RemoveFeature(pSpriteFeature);
			pSpriteFeature?.Dispose();

			if(pSpriteBatch != null)
				pSpriteBatch.OnDraw -= OnDraw;
		}

		public void Load(IServiceProvider provider)
		{
			// ReSharper disable once StringLiteralTypo
			pSpriteEffect.PixelShader = new FileShaderStream(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Shaders/sprite_smpl_effect_ps.hlsl"));

			// Load Sprite
			ImageAsset sprite = new("doge.png");
			using (FileStream stream = new(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Textures/doge.jpg"), FileMode.Open))
				sprite.Load(stream).Wait();

			pRenderer = provider.Get<IRenderer>();
			pSpriteBatch = provider.Get<ISpriteBatch>();
			pSpriteBatch.SetTexture(0, sprite.Image);

			pRenderer.AddFeature(pSpriteFeature = pSpriteBatch.Feature);
			pSpriteBatch.OnDraw += OnDraw;
		}

		private void OnDraw(object? sender, EventArgs e)
		{
			if (pSpriteBatch is null || Window is null)
				return;

			var size = Window.Size;
			pSpriteBatch.Draw(new SpriteBatchInfo
			{
				Effect = pSpriteEffect,
				Anchor = new Vector2(0.5f, 0.5f),
				Position = size.ToVector2() * new Vector2(0.5f, 0.5f),
				TextureSlot = 0
			});
		}

		public void Update(IServiceProvider provider)
		{
		}
	}
}
