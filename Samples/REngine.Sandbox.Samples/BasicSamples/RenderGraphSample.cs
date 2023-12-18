using ImGuiNET;
using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RPI;
using REngine.RPI.Features;
using REngine.RPI.RenderGraph;
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
	[Sample("Render Graph")]
	internal class RenderGraphSample(
		IRenderer renderer, 
		IRenderGraph renderGraph,
		IVariableManager variableManager,
		IImGuiSystem imGuiSystem,
		ISpriteBatch spriteBatch,
		IAssetManager assetManager) : ISample
	{
		private IRenderFeature? pFeature;

		private TextRendererBatch? pTextBatch;

		private IVar? pEnableSpritebatchVar;
		private bool pEnableSpritebatch = false;
		private int pBounceTimes = 0;

		private RectangleF pSpriteRect = new RectangleF(50, 50, 100, 100);

		public IWindow? Window { get; set; }

		public void Dispose()
		{
			renderer.RemoveFeature(pFeature);
			pFeature?.Dispose();

			imGuiSystem.OnGui -= OnGui;
			renderer?.AddFeature(imGuiSystem.Feature, 100);

			spriteBatch.OnDraw -= OnDraw;
			spriteBatch.ClearTexture(0);
			pTextBatch?.Dispose();

			GC.SuppressFinalize(this);
		}

		public void Load(IServiceProvider provider)
		{
			if (Window is null)
				return;

			// Load Sprite
			var sprite = assetManager.GetAsset<ImageAsset>("Textures/doge.jpg");
			spriteBatch.SetTexture(0, sprite.Image);

			// Load Font
			var fontAsset = assetManager.GetAsset<FontAsset>("Fonts/Anonymous-Pro.ttf");
			pTextBatch = provider.Get<ITextRenderer>().SetFont(fontAsset.Font).CreateBatch(fontAsset.Font.Name);
			pTextBatch.Text = "Render Graph Sample";
			pTextBatch.Size = 24;
			pTextBatch.Position = (Window.Size * 0.5f).ToVector2();
			
			pEnableSpritebatchVar = variableManager.GetVar("@vars/spritebatch/enabled");

			// ReSharper disable once StringLiteralTypo
			var rootEntry = renderGraph.Load("default-rendergraph.xml");

			pFeature = new RenderGraphFeature(renderGraph, rootEntry);
			renderer.AddFeature(pFeature);
			// Remove ImGui feature, otherwise we will deal with double rendering
			renderer.RemoveFeature(
				imGuiSystem.Feature
			);

			imGuiSystem.OnGui += OnGui;
			spriteBatch.OnDraw += OnDraw;
		}

		private void OnDraw(object? sender, EventArgs e)
		{
			var bounds = pSpriteRect;
			spriteBatch.Draw(new SpriteBatchInfo
			{
				TextureSlot =0,
				Position = bounds.GetPosition(),
				Size = bounds.GetSize().ToVector2(),
			});
			spriteBatch.Draw(pTextBatch);
		}

		private void OnGui(object? sender, EventArgs e)
		{
			ImGui.Begin("Render Graph Settings");
			ImGui.Checkbox("Enable Spritebatch", ref pEnableSpritebatch);
			ImGui.End();
		}

		private Vector2 pVelocity = Vector2.One;
		private Vector2 pSpriteVelocity = new Vector2(1, -1);
		private float pColorAngle = 0;

		public void Update(IServiceProvider provider)
		{
			if (Window is null || pTextBatch is null)
				return;

			if(pEnableSpritebatchVar != null)
				pEnableSpritebatchVar.Value = pEnableSpritebatch;

			var bounds = pTextBatch.Bounds;
			var pos = pTextBatch.Position;

			HandleCollisionItem(ref pos, bounds.Size, ref pVelocity, pSpriteRect);
			pos += pVelocity;

			if (HandleCollision(ref pos, bounds.Size, ref pVelocity))
			{
				pTextBatch.Color = ColorUtils.FromHSL(pColorAngle, 1.0f, 1.0f);
				pColorAngle += 0.1f;
				pBounceTimes++;
				pTextBatch.Text = $"Render Graph Sample. Bounce: {pBounceTimes}";
			}

			pTextBatch.Position = pos;

			bounds = pSpriteRect;
			pos = pSpriteRect.GetPosition();

			HandleCollisionItem(ref pos, bounds.Size, ref pSpriteVelocity, pTextBatch.Bounds);
			pos += pSpriteVelocity;

			if(HandleCollision(ref pos, bounds.Size, ref pSpriteVelocity))
			{
				float increaseRatio = pBounceTimes / 10.0f;
				double size = 100 + ((0.5 - (Math.Cos(increaseRatio * Math.PI) / 2.0)) * 100);
				bounds.Width = bounds.Height = (int)size;
			}

			bounds.X = pos.X;
			bounds.Y = pos.Y;

			pSpriteRect = bounds;
		}

		private bool HandleCollision(ref Vector2 input, in SizeF size, ref Vector2 velocity)
		{
			if (Window is null)
				return false;

			Vector2 newVelocity;
			Vector2 normal = Vector2.Zero;
			float offset = 5f;
			if ((input.X <= 0 || input.X + size.Width >= Window.Size.Width) || (input.Y <= 0 || input.Y + size.Height >= Window.Size.Height))
			{
				if (input.X <= 0)
				{
					normal.X = 1;
					input.X = offset;
				}
				else if (input.X + size.Width >= Window.Size.Width)
				{
					normal.X = -1;
					input.X = Window.Size.Width - (size.Width + offset);
				}

				if (input.Y <= 0)
				{
					normal.Y = -1;
					input.Y = offset;
				}
				else if(input.Y + size.Height >= Window.Size.Height)
				{
					normal.Y = 1;
					input.Y = Window.Size.Height - (size.Height + offset);
				}

				newVelocity = Vector2.Normalize(Vector2.Reflect(velocity, normal));
				bool collided = newVelocity != velocity;
				velocity = newVelocity;
				return collided;
			}

			return false;
		}
	
		private void HandleCollisionItem(ref Vector2 position, in SizeF size, ref Vector2 velocity, in RectangleF collider)
		{
			RectangleF bounds = new (position.X, position.Y, size.Width, size.Height);

			if (bounds.IntersectsWith(collider))
			{
				Vector2 normal = Vector2.Normalize(new Vector2(1, 0));
				velocity = Vector2.Reflect(-velocity, normal);
			}
		}
	}
}
