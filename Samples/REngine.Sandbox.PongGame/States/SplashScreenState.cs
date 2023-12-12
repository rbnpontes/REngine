using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using REngine.Assets;
using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Logic;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;
using REngine.RPI;
using REngine.RPI.Components;
using REngine.RPI.RenderGraph;

namespace REngine.Sandbox.PongGame.States
{
	public class SplashScreenState(
		ISpriteBatch spriteBatch,
		EntityManager entityManager,
		IWindow mainWindow,
		RenderState renderState,
		GameStateManager gameStateManager,
		IVariableManager variableManager,
		IAssetManager assetManager)
		: IGameState
	{
		private readonly Color pDefaultClearColor = renderState.DefaultClearColor;
		private readonly IVar pEnableCrt = variableManager.GetVar("@vars/pong/enable_crt");
		public string Name => PongStates.SplashScreenState;

		private Transform2D? pComponent;
		private Asset? pAudioAsset;
		private IAudio? pAudio;

		public void OnStart()
		{
			mainWindow.Fullscreen();
			var sprite = assetManager.GetAsset<ImageAsset>("Textures/EngineLogo-Sdf.png");
			spriteBatch.SetTexture(0, sprite.Image);

			var audioAsset = assetManager.GetAsset<StreamedAudioAsset>("Sounds/doge_bonk.ogg");
			pAudio = audioAsset.Audio;
			pAudioAsset = audioAsset;

			var effect = new BasicSpriteEffect("Engine Effect", assetManager);
			effect.PixelShader =
				new FileShaderStream(Path.Join(EngineSettings.AssetsShadersPath, "engine_logo_effect.hlsl"));

			var entity = entityManager.CreateEntity("Engine Logo");
			entity.Enabled = false;
			pComponent = entity.CreateComponent<Transform2D>();

			var spriteComponent = entity.CreateComponent<SpriteComponent>();
			spriteComponent.Anchor = new Vector2(0.5f, 0.5f);
			spriteComponent.TextureSlot = 0;
			spriteComponent.Effect = effect;

			var winScale = mainWindow.Size.ToVector2();
			pComponent.Position = winScale * new Vector2(0.5f, 0.5f);
			winScale *= new Vector2(0.4f);
			pComponent.Scale = new Vector2(Math.Min(winScale.X, winScale.Y));

			pEnableCrt.Value = new Ref<bool>(false);
		}

		private readonly Stopwatch pStopwatch = new();
		private bool pReady;
		public void OnUpdate()
		{
			if (pComponent is null)
				return;

			if (pAudio is null)
				return;

			if (pAudio.State == AudioState.Stopped && pReady)
			{
				// Goto next State
				pAudio = null;
				gameStateManager.SetState(PongStates.LoadingPongState);
				return;
			}

			if (pAudio.State != AudioState.Playing)
			{
				pAudio.Play();
				pStopwatch.Start();
				pReady = true;
				return;
			} 
			

			// Enable Component After Doge Bonk
			if (pStopwatch.Elapsed.TotalSeconds > 1 && pComponent.Owner != null)
				pComponent.Owner.Enabled = true;

			var winScale = mainWindow.Size.ToVector2();
			pComponent.Position = winScale * new Vector2(0.5f, 0.5f);
			winScale *= new Vector2(0.4f);
			pComponent.Scale = new Vector2(Math.Min(winScale.X, winScale.Y));

			renderState.DefaultClearColor = Color.White;
		}

		public void OnExit()
		{
			pEnableCrt.Value = new Ref<bool>(true);
			pStopwatch.Stop();
			renderState.DefaultClearColor = pDefaultClearColor;
			entityManager.DestroyAll();
			if(pAudioAsset != null)
				assetManager.UnloadAsset(pAudioAsset);
		}
	}
}
