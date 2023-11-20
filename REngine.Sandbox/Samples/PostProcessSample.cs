using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.WorldManagement;
using REngine.RPI;
using REngine.RPI.Features;
using REngine.RPI.RenderGraph;

namespace REngine.Sandbox.Samples
{
	[Sample("Post Process Sample")]
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
		}

		public void Load(IServiceProvider provider)
		{
			if (Window is null)
				return;

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
			for (int i = 0; i < 100; ++i)
			{
				var x = (float)rnd.NextDouble() * size.Width;
				var y = (float)rnd.NextDouble() * size.Height;
				var rot = (float)rnd.NextDouble();

				var entity = pEntityMgr.GetEntity($"Sprite #{i}");
				var transform = entity.CreateComponent<Transform2D>();
				var sprite = entity.CreateComponent<SpriteComponent>();
			}
		}

		private void OnGui(object? sender, EventArgs e)
		{

		}

		public void Update(IServiceProvider provider)
		{
		}
	}
}
