using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Assets;
using REngine.Core;
using REngine.Core.Logic;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;
using REngine.RPI;
using REngine.RPI.Components;

namespace REngine.Sandbox.States
{
	internal class LoadPongState : IGameState
	{
		private const float LoadingWidth = 200;
		private const float LoadingHeight = 30;
		private const float LoadingPaddingSize = 8;

		private readonly Queue<Action> pLoadQueue = new();
		private readonly GameStateManager pGameStateManager;
		private readonly EntityManager pEntityManager;
		private readonly RenderState pRenderState;
		private readonly IWindow pMainWindow;
		private readonly ISpriteBatch pSpriteBatch;

		private Color pDefaultClearColor = Color.Black;

		private Transform2D? pLoadingTransform;
		private Transform2D? pBarTransform;

		private int pLoadCount = 0;
		public string Name => nameof(LoadPongState);


		public LoadPongState(
			GameStateManager gameStateManager,
			EntityManager entityManager,
			RenderState renderState,
			IWindow mainWindow,
			ISpriteBatch spriteBatch
		)
		{
			pGameStateManager = gameStateManager;
			pEntityManager = entityManager;
			pRenderState = renderState;
			pMainWindow = mainWindow;
			pSpriteBatch = spriteBatch;
		}

		public void OnStart()
		{
			pDefaultClearColor = pRenderState.DefaultClearColor;

			pLoadQueue.Enqueue(()=> PongVariables.BackgroundAudio = LoadAudio("silent_wood_by_purrplecat.ogg"));
			pLoadQueue.Enqueue(()=> PongVariables.MenuItemAudio = LoadAudio("menu_selection.ogg"));
			pLoadQueue.Enqueue(()=> LoadImage("menu-play-button.png", PongVariables.MenuPlayButtonSlot));
			pLoadQueue.Enqueue(()=> LoadImage("menu-exit-button.png", PongVariables.MenuExitButtonSlot));
			pLoadQueue.Enqueue(()=> LoadImage("menu-restart-button.png", PongVariables.MenuRestartButtonSlot));

			pLoadCount = pLoadQueue.Count;

			SetupLoadSprites();
		}

		private void SetupLoadSprites()
		{
			var root = pEntityManager.CreateEntity("Root Loading");
			var rootTransform = root.CreateComponent<Transform2D>();
			pLoadingTransform = rootTransform;

			var spriteEntity = pEntityManager.CreateEntity("Loading Base");
			var spriteTransform = spriteEntity.CreateComponent<Transform2D>();
			spriteTransform.Scale = new Vector2(LoadingWidth + LoadingPaddingSize * 2, LoadingHeight + LoadingPaddingSize * 2);

			rootTransform.AddChild(spriteTransform);

			var sprite = spriteEntity.CreateComponent<SpriteComponent>();
			sprite.Color = Color.Black;

			spriteEntity = pEntityManager.CreateEntity("Loading Bar");
			pBarTransform = spriteEntity.CreateComponent<Transform2D>();
			pBarTransform.Position = new Vector2(LoadingPaddingSize);
			pBarTransform.Scale = new Vector2(0, LoadingHeight);

			rootTransform.AddChild(pBarTransform);

			sprite = spriteEntity.CreateComponent<SpriteComponent>();
			sprite.Color = Color.White;
		}

		public void OnUpdate()
		{
			pRenderState.DefaultClearColor = Color.White;
			if (pBarTransform is null || pLoadingTransform is null)
				return;

			// Center Sprite Bar
			var pos = pMainWindow.Size.ToVector2() * 0.5f - new Vector2(LoadingWidth * 0.5f, LoadingHeight * 0.5f);
			pLoadingTransform.Position = pos;

			if (pLoadQueue.TryDequeue(out var action))
				action();
			else
			{
				// If queue items is empty, then we must go to next state
				pGameStateManager.SetState(PongStates.PongMainMenuState);
				return;
			}

			var progress = (float)pLoadQueue.Count / pLoadQueue.Count;
			pBarTransform.Scale = new Vector2(progress * LoadingWidth, LoadingHeight);
		}

		public void OnExit()
		{
			pRenderState.DefaultClearColor = pDefaultClearColor;
			pEntityManager.DestroyAll();
		}

		private IAudio LoadAudio(string assetName)
		{
			var audioAsset = new StreamedAudioAsset();
			audioAsset.Load(new FileStream(Path.Join(EngineSettings.AssetsSoundsPath, assetName), FileMode.Open,
				FileAccess.Read)).Wait();
			if (audioAsset.Audio is null)
				throw new NullReferenceException($"Error has occurred while is loading {assetName}");

			PongVariables.Assets2Dispose.Enqueue(audioAsset);
			return audioAsset.Audio;
		}

		private void LoadImage(string assetName, byte slotId)
		{
			var imageAsset = new ImageAsset(assetName);
			using (FileStream stream = new(Path.Join(EngineSettings.AssetsTexturesPath, assetName), FileMode.Open,
				       FileAccess.Read))
				imageAsset.Load(stream).Wait();

			var img = imageAsset.Image;
			pSpriteBatch.SetTexture(slotId, img);
		}
	}
}
