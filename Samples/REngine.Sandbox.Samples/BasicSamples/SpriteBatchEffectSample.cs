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
using REngine.Core.Resources;
using REngine.RPI;
using REngine.Sandbox.BaseSample;

namespace REngine.Sandbox.Samples.BasicSamples
{
	[Sample("SpriteBatch - Effect")]
	internal class SpriteBatchEffectSample(
		IRenderer renderer,
		ISpriteBatch spriteBatch,
		IAssetManager assetManager) : ISample
	{
		private readonly BasicSpriteEffect pSpriteEffect = new("Sample Effect", assetManager);

		private IRenderFeature? pSpriteFeature;
		public IWindow? Window { get; set; }
		public void Dispose()
		{
			pSpriteEffect.Dispose();

			renderer.RemoveFeature(pSpriteFeature);
			pSpriteFeature?.Dispose();

			spriteBatch.OnDraw -= OnDraw;
		}

		public void Load(IServiceProvider provider)
		{
			// ReSharper disable once StringLiteralTypo
			pSpriteEffect.PixelShader = new StreamedShaderStream(assetManager.GetStream("Shaders/sprite_smpl_effect_ps.hlsl"));

			// Load Sprite
			var sprite = assetManager.GetAsset<ImageAsset>("Textures/doge.jpg");
			renderer = provider.Get<IRenderer>();
			spriteBatch = provider.Get<ISpriteBatch>();
			spriteBatch.SetTexture(0, sprite.Image);

			renderer.AddFeature(pSpriteFeature = spriteBatch.Feature);
			spriteBatch.OnDraw += OnDraw;
		}

		private void OnDraw(object? sender, EventArgs e)
		{
			if (Window is null)
				return;

			var size = Window.Size;
			spriteBatch.Draw(new SpriteBatchInfo
			{
				Effect = pSpriteEffect,
				Anchor = new Vector2(0.5f, 0.5f),
				Size = new Vector2(200, 200),
				Position = size.ToVector2() * new Vector2(0.5f, 0.5f),
				TextureSlot = 0
			});
		}

		public void Update(IServiceProvider provider)
		{
		}
	}
}
