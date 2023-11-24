using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
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

namespace REngine.Sandbox.States
{
	public class SplashScreenState : IGameState
	{
		private readonly ISpriteBatch pSpriteBatch;
		private readonly EntityManager pEntityManager;
		private readonly IWindow pMainWindow;
		private readonly RenderState pRenderState;
		private readonly Color pDefaultClearColor;
		private readonly GameStateManager pGameStateManager;
		public string Name => PongStates.SplashScreenState;

		private Transform2D? pComponent;
		private IAsset? pAudioAsset;
		private IAudio? pAudio;

		public SplashScreenState(
			ISpriteBatch spriteBatch,
			EntityManager entityManager,
			IWindow mainWindow,
			RenderState renderState,
			GameStateManager gameStateManager
		)
		{
			pSpriteBatch = spriteBatch;
			pEntityManager = entityManager;
			pMainWindow = mainWindow;
			pRenderState = renderState;
			pDefaultClearColor = renderState.DefaultClearColor;
			pGameStateManager = gameStateManager;
		}
		public void OnStart()
		{
			//pMainWindow.Fullscreen();
			using ImageAsset sprite = new("EngineLogo-Sdf.png");
			using (FileStream stream = new(Path.Join(EngineSettings.AssetsPath, "Textures", sprite.Name),
				       FileMode.Open, FileAccess.Read))
				sprite.Load(stream).Wait();
			pSpriteBatch.SetTexture(0, sprite.Image);

			var audioAsset = new StreamedAudioAsset();
			audioAsset.Load(new FileStream(Path.Join(EngineSettings.AssetsSoundsPath, "doge_bonk.ogg"), FileMode.Open,
				FileAccess.Read)).Wait();
			pAudio = audioAsset.Audio;
			pAudioAsset = audioAsset;

			var effect = new BasicSpriteEffect("Engine Effect");
			effect.PixelShader =
				new FileShaderStream(Path.Join(EngineSettings.AssetsShadersPath, "engine_logo_effect.hlsl"));

			var entity = pEntityManager.CreateEntity("Engine Logo");
			entity.Enabled = false;
			pComponent = entity.CreateComponent<Transform2D>();

			var spriteComponent = entity.CreateComponent<SpriteComponent>();
			spriteComponent.Anchor = new Vector2(0.5f, 0.5f);
			spriteComponent.TextureSlot = 0;
			spriteComponent.Effect = effect;

			var winScale = pMainWindow.Size.ToVector2();
			pComponent.Position = winScale * new Vector2(0.5f, 0.5f);
			winScale *= new Vector2(0.4f);
			pComponent.Scale = new Vector2(Math.Min(winScale.X, winScale.Y));
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
				pGameStateManager.SetState(PongStates.LoadingPongState);
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

			var winScale = pMainWindow.Size.ToVector2();
			pComponent.Position = winScale * new Vector2(0.5f, 0.5f);
			winScale *= new Vector2(0.4f);
			pComponent.Scale = new Vector2(Math.Min(winScale.X, winScale.Y));

			pRenderState.DefaultClearColor = Color.White;
		}

		public void OnExit()
		{
			pStopwatch.Stop();
			pRenderState.DefaultClearColor = pDefaultClearColor;
			pEntityManager.DestroyAll();
			pAudioAsset?.Dispose();
		}
	}
}
