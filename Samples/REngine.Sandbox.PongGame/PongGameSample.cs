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
using REngine.Sandbox.BaseSample;
using REngine.Sandbox.PongGame.States;

namespace REngine.Sandbox.PongGame
{
	[Sample("Pong Game")]
	public sealed class PongGameSample(
		GameStateManager gameStateManager,
		IRenderer renderer,
		IRenderGraph renderGraph,
		IImGuiSystem imGuiSystem)
		: ISample
	{
		private IRenderFeature? pFeature;
		
		public IWindow? Window { get; set; }

		public void Dispose()
		{
			gameStateManager.Stop().ClearStates();
			
			PongVariables.Reset();

			renderer.RemoveFeature(pFeature);
			pFeature.Dispose();

			renderer.AddFeature(imGuiSystem.Feature, 100);
		}

		public void Load(IServiceProvider provider)
		{
			gameStateManager
				.RegisterState<SplashScreenState>()
				.RegisterState<LoadPongState>()
				.RegisterState<PongMainMenuState>()
				.RegisterState<PongGamePlayState>()
				.RegisterState<PongGameOverState>();

			// ReSharper disable once StringLiteralTypo
			var rootEntry = renderGraph.Load("ponggame-rendergraph.xml");

			pFeature = new RenderGraphFeature(renderGraph, rootEntry);
			renderer.AddFeature(pFeature);

			renderer.RemoveFeature(imGuiSystem.Feature);

			//pGameStateManager.SetState(PongStates.PongGamePlayState);
			gameStateManager.SetState(PongStates.SplashScreenState); // Start Game
		}

		public void Update(IServiceProvider provider)
		{
		}
	}
}
