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

namespace REngine.Sandbox.Samples
{
	[Sample("Render Graph")]
	internal class RenderGraphSample : ISample
	{
		private IVariableManager? pVarMgr;
		private IRenderGraph? pRenderGraph;
		private IRenderFeature? pFeature;
		private IRenderer? pRenderer;
		private IImGuiSystem? pImGuiSystem;
		private ISpriteBatch? pSpritebatch;

		private TextRendererBatch? pTextBatch;

		private IVar? pEnableSpritebatchVar;
		private bool pEnableSpritebatch = false;
		private int pBounceTimes = 0;

		private RectangleF pSpriteRect = new RectangleF(50, 50, 100, 100);

		public IWindow? Window { get; set; }

		public void Dispose()
		{
			pRenderer?.RemoveFeature(pFeature);
			pFeature?.Dispose();

			if(pImGuiSystem != null)
				pRenderer?.AddFeature(pImGuiSystem.Feature);

			pTextBatch?.Dispose();
		}

		public void Load(IServiceProvider provider)
		{
			if (Window is null)
				return;
			pRenderer = provider.Get<IRenderer>();
			pRenderGraph = provider.Get<IRenderGraph>();
			pVarMgr = provider.Get<IVariableManager>();
			pImGuiSystem = provider.Get<IImGuiSystem>();
			pSpritebatch = provider.Get<ISpriteBatch>();

			// Load Sprite
			ImageAsset sprite = new("doge.png");
			using (FileStream stream = new(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Textures/doge.jpg"), FileMode.Open))
				sprite.Load(stream).Wait();

			pSpritebatch.SetTexture(0, sprite.Image);

			// Load Font
			FontAsset fontAsset = new();
			fontAsset.Name = "Anonymous Pro";
			using (FileStream stream = new(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Fonts/Anonymous Pro.ttf"), FileMode.Open))
				fontAsset.Load(stream).Wait();

			pTextBatch = provider.Get<ITextRenderer>().SetFont(fontAsset.Font).CreateBatch(fontAsset.Font.Name);
			pTextBatch.Text = "Render Graph Sample";
			pTextBatch.Size = 24;
			pTextBatch.Position = (Window.Size * 0.5f).ToVector2();

			pEnableSpritebatchVar = pVarMgr.GetVar("@vars/spritebatch/enabled");

			var rootEntry = pRenderGraph.LoadFromFile(
				Path.Join(
					AppDomain.CurrentDomain.BaseDirectory,
					"Assets/default-rendergraph.xml"
				)
			);

			pFeature = new RenderGraphFeature(pRenderGraph, rootEntry);
			pRenderer.AddFeature(pFeature);
			// Remove ImGui feature, otherwise we will deal with double rendering
			pRenderer.RemoveFeature(
				pImGuiSystem.Feature
			);

			pImGuiSystem.OnGui += OnGui;
			pSpritebatch.OnDraw += OnDraw;
		}

		private void OnDraw(object? sender, EventArgs e)
		{
			var bounds = pSpriteRect;
			pSpritebatch?.Draw(new SpriteBatchInfo
			{
				TextureSlot =0,
				Position = bounds.GetPosition(),
				Size = bounds.GetSize().ToVector2(),
			});
			pSpritebatch?.Draw(pTextBatch);
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
