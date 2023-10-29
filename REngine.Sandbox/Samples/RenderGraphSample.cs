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
			pRenderer = provider.Get<IRenderer>();
			pRenderGraph = provider.Get<IRenderGraph>();
			pVarMgr = provider.Get<IVariableManager>();
			pImGuiSystem = provider.Get<IImGuiSystem>();
			pSpritebatch = provider.Get<ISpriteBatch>();

			// Load Font
			FontAsset fontAsset = new();
			fontAsset.Name = "Anonymous Pro";
			using (FileStream stream = new(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Fonts/Anonymous Pro.ttf"), FileMode.Open))
				fontAsset.Load(stream).Wait();

			pTextBatch = provider.Get<ITextRenderer>().SetFont(fontAsset.Font).CreateBatch(fontAsset.Font.Name);
			pTextBatch.Text = "Render Graph Sample";
			pTextBatch.Size = 24;

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
			pSpritebatch?.Draw(pTextBatch);
		}

		private void OnGui(object? sender, EventArgs e)
		{
			ImGui.Begin("Render Graph Settings");
			ImGui.Checkbox("Enable Spritebatch", ref pEnableSpritebatch);
			ImGui.End();
		}


		private Vector2 pVelocity = Vector2.One;
		private float pColorAngle = 0;
		public void Update(IServiceProvider provider)
		{
			if (Window is null || pTextBatch is null)
				return;
			if(pEnableSpritebatchVar != null)
				pEnableSpritebatchVar.Value = pEnableSpritebatch;

			var bounds = pTextBatch.Bounds;
			var pos = pTextBatch.Position;
			pos += pVelocity;

			bool collided = false;
			
			if (pos.X + bounds.Width >= Window.Size.Width)
			{
				pVelocity.X = -1;
				collided = true;
			}
			else if (pos.X <= 0)
			{
				pVelocity.X = 1;
				collided = true;
			}

			if (pos.Y + bounds.Height >= Window.Size.Height)
			{
				pVelocity.Y = -1;
				collided = true;
			}
			else if (pos.Y <= 0)
			{
				pVelocity.Y = 1;
				collided = true;
			}

			if(collided)
			{
				pTextBatch.Color = ColorUtils.FromHSL(pColorAngle, 1.0f, 1.0f);
				pColorAngle += 0.1f;
			}

			pTextBatch.Position = pos;
		}
	}
}
