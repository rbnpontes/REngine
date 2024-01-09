using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;
using REngine.Game.Components;
using REngine.RPI;
using REngine.RPI.Components;
using REngine.RPI.Features;
using REngine.RPI.RenderGraph;
using REngine.RPI.Resources;
using REngine.Sandbox.BaseSample;

namespace REngine.Sandbox.Samples.BasicSamples
{
	[Sample("Post Process")]
	internal class PostProcessSample(
		IVariableManager varMgr,
		IRenderGraph renderGraph,
		IRenderer renderer,
		IImGuiSystem imGuiSys,
		EntityManager entityMgr,
		ISpriteBatch spriteBatch,
		IAssetManager assetManager)
		: ISample
	{
		private readonly IVariableManager pVarMgr = varMgr;
		private readonly IVar pVar = varMgr.GetVar("@vars/sample/postprocess-effect");

		private IRenderFeature? pFeature;
		private SpriteEffect? pEffect;
		public IWindow? Window { get; set; }


		public void Dispose()
		{
			renderer.RemoveFeature(pFeature);
			pFeature.Dispose();

			entityMgr.DestroyAll();

			imGuiSys.OnGui -= OnGui;
			renderer.AddFeature(imGuiSys.Feature, 100);
			
			pEffect?.Dispose();
		}

		public void Load(IServiceProvider provider)
		{
			if (Window is null)
				return;

			// Load Sprite
			var sprite = assetManager.GetAsset<TextureAsset>("Textures/doge.jpg");
			var effect = TextureSpriteEffect.Build(provider);
			effect.Texture = sprite.Texture;
			pEffect = effect;
			
			var rootEntry = renderGraph.Load("postprocess-rendergraph.xml");

			pFeature = new RenderGraphFeature(renderGraph, rootEntry);
			renderer.AddFeature(pFeature);

			renderer.RemoveFeature(imGuiSys.Feature);

			imGuiSys.OnGui += OnGui;

			CreateSprites(Window.Size);
		}

		private void CreateSprites(Size size)
		{
			var rnd = new Random();
			for (var i = 0; i < 100; ++i)
			{
				var x = (float)rnd.NextDouble() * size.Width;
				var y = (float)rnd.NextDouble() * size.Height;
				var scale = 10 + (float)rnd.NextDouble() * 100;
				var rot = (float)rnd.NextDouble();

				var entity = entityMgr.CreateEntity($"Sprite #{i}");
				var transform = entity.CreateComponent<Transform2D>();
				var sprite = entity.CreateComponent<SpriteComponent>();
				sprite.Anchor = new Vector2(0.5f);

				transform.Scale = new Vector2(scale);
				transform.Position = new Vector2(x, y);
				transform.Rotation = rot;

				sprite.Effect = pEffect;
			}
		}

		private int pPostProcessOption = 0;
		private void OnGui(object? sender, EventArgs e)
		{
			ImGui.Begin("Post Process Sample");
			ImGui.RadioButton("Grayscale", ref pPostProcessOption, 0);
			ImGui.RadioButton("Invert", ref pPostProcessOption, 1);
			ImGui.RadioButton("Sepia", ref pPostProcessOption, 2);
			ImGui.End();

			pVar.Value = pPostProcessOption;
		}

		public void Update(IServiceProvider provider)
		{
		}
	}
}
