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
using REngine.Core.WorldManagement;
using REngine.RPI;
using REngine.RPI.Components;
using REngine.RPI.Features;
using REngine.RPI.RenderGraph;

namespace REngine.Sandbox.Samples
{
	[Sample("Post Process")]
	internal class PostProcessSample : ISample
	{
		private readonly IVariableManager pVarMgr;
		private readonly IRenderGraph pRenderGraph;
		private readonly IRenderer pRenderer;
		private readonly IImGuiSystem pImGuiSystem;
		private readonly EntityManager pEntityMgr;
		private readonly IVar pVar;

		private IRenderFeature? pFeature;
		public IWindow? Window { get; set; }


		public PostProcessSample(
			IVariableManager varMgr,
			IRenderGraph renderGraph,
			IRenderer renderer,
			IImGuiSystem imGuiSys,
			EntityManager entityMgr
		)
		{
			pVarMgr = varMgr;
			pRenderGraph = renderGraph;
			pRenderer = renderer;
			pImGuiSystem = imGuiSys;
			pEntityMgr = entityMgr;

			pVar = varMgr.GetVar("@vars/sample/postprocess-effect");
		}

		public void Dispose()
		{
			pRenderer.RemoveFeature(pFeature);
			pFeature.Dispose();

			pEntityMgr.DestroyAll();

			pImGuiSystem.OnGui -= OnGui;
			pRenderer.AddFeature(pImGuiSystem.Feature, 100);
		}

		public void Load(IServiceProvider provider)
		{
			if (Window is null)
				return;

			// Load Sprite
			ImageAsset sprite = new("doge.png");
			using (FileStream stream = new(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Textures/doge.jpg"), FileMode.Open))
				sprite.Load(stream).Wait();

			provider.Get<ISpriteBatch>().SetTexture(0, sprite.Image);


			var rootEntry = pRenderGraph.LoadFromFile(
				Path.Join(EngineSettings.AssetsPath, "postprocess-rendergraph.xml")
			);

			pFeature = new RenderGraphFeature(pRenderGraph, rootEntry);
			pRenderer.AddFeature(pFeature);

			pRenderer.RemoveFeature(pImGuiSystem.Feature);

			pImGuiSystem.OnGui += OnGui;

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

				var entity = pEntityMgr.CreateEntity($"Sprite #{i}");
				var transform = entity.CreateComponent<Transform2D>();
				var sprite = entity.CreateComponent<SpriteComponent>();
				sprite.Anchor = new Vector2(0.5f);

				transform.Scale = new Vector2(scale);
				transform.Position = new Vector2(x, y);
				transform.Rotation = rot;

				sprite.TextureSlot = 0;
			}
		}

		private int pPostProcessOption = 0;
		private void OnGui(object? sender, EventArgs e)
		{
			ImGui.Begin("Post Process Sample");
			ImGui.RadioButton("Grayscale", ref pPostProcessOption, 0);
			ImGui.RadioButton("Invert", ref pPostProcessOption, 1);
			ImGui.End();

			pVar.Value = pPostProcessOption;
		}

		public void Update(IServiceProvider provider)
		{
		}
	}
}
