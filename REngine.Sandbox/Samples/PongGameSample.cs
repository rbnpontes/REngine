using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.Logic;
using REngine.RPI;
using REngine.RPI.Features;
using REngine.RPI.RenderGraph;
using REngine.Sandbox.States;

namespace REngine.Sandbox.Samples
{
	[Sample("Pong Game")]
	internal class PongGameSample : ISample
	{
		private readonly GameStateManager pGameStateManager;
		private readonly IRenderer pRenderer;
		private readonly IRenderGraph pRenderGraph;
		private readonly IImGuiSystem pImGuiSystem;

		private IRenderFeature? pFeature;
		
		public IWindow? Window { get; set; }

		public PongGameSample(
			GameStateManager gameStateManager, 
			IRenderer renderer,
			IRenderGraph renderGraph,
			IImGuiSystem imGuiSystem)
		{
			pGameStateManager = gameStateManager;
			pRenderer = renderer;
			pRenderGraph = renderGraph;
			pImGuiSystem = imGuiSystem;
		}
		public void Dispose()
		{
			pGameStateManager.Stop().ClearStates();
			
			PongVariables.Reset();

			pRenderer.RemoveFeature(pFeature);
			pFeature.Dispose();

			pRenderer.AddFeature(pImGuiSystem.Feature, 100);
		}

		public void Load(IServiceProvider provider)
		{
			pGameStateManager
				.RegisterState<SplashScreenState>()
				.RegisterState<LoadPongState>()
				.RegisterState<PongMainMenuState>()
				.RegisterState<PongGamePlayState>();

			var rootEntry = pRenderGraph.LoadFromFile(
				Path.Join(EngineSettings.AssetsPath, "ponggame-rendergraph.xml")
			);

			pFeature = new RenderGraphFeature(pRenderGraph, rootEntry);
			pRenderer.AddFeature(pFeature);

			pRenderer.RemoveFeature(pImGuiSystem.Feature);

			//pGameStateManager.SetState(PongStates.PongGamePlayState);
			pGameStateManager.SetState(PongStates.SplashScreenState); // Start Game
		}

		public void Update(IServiceProvider provider)
		{
		}
	}
}
