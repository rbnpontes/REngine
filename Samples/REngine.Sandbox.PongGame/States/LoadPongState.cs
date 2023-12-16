using System.Drawing;
using System.Numerics;
using REngine.Assets;
using REngine.Core;
using REngine.Core.Logic;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;
using REngine.RHI;
using REngine.RPI;
using REngine.RPI.Components;
using REngine.RPI.RenderGraph;

namespace REngine.Sandbox.PongGame.States
{
	internal class LoadPongState(
		GameStateManager gameStateManager,
		EntityManager entityManager,
		RenderState renderState,
		IWindow mainWindow,
		ISpriteBatch spriteBatch,
		ITextRenderer textRenderer,
		IResourceManager resourceManager,
		IAssetManager assetManager)
		: IGameState
	{
		private const float LoadingWidth = 200;
		private const float LoadingHeight = 30;
		private const float LoadingPaddingSize = 8;

		private readonly Queue<Action> pLoadQueue = new();

		private Color pDefaultClearColor = Color.Black;

		private Transform2D? pLoadingTransform;
		private Transform2D? pBarTransform;

		private int pLoadCount;
		public string Name => nameof(LoadPongState);


		public void OnStart()
		{
			pDefaultClearColor = renderState.DefaultClearColor;

			pLoadQueue.Enqueue(()=> PongVariables.BackgroundAudio = LoadAudio("silent_wood_by_purrplecat.ogg"));
			pLoadQueue.Enqueue(()=> PongVariables.MenuItemAudio = LoadAudio("menu_selection.ogg", false));
			pLoadQueue.Enqueue(()=> PongVariables.BlockClickAudio = LoadAudio("block_click.ogg", false));
			pLoadQueue.Enqueue(()=> LoadImage("menu-play-button.png", PongVariables.MenuPlayButtonSlot));
			pLoadQueue.Enqueue(()=> LoadImage("menu-exit-button.png", PongVariables.MenuExitButtonSlot));
			pLoadQueue.Enqueue(()=> LoadImage("menu-restart-button.png", PongVariables.MenuRestartButtonSlot));
			pLoadQueue.Enqueue(()=> LoadImage("menu-resume-button.png", PongVariables.MenuResumeButtonSlot));
			pLoadQueue.Enqueue(()=> LoadFont(PongVariables.DefaultFont));
			pLoadQueue.Enqueue(() =>
			{
				// Load blur screen into sprite batch
				var resource = resourceManager.GetResource("@sample/blur");
				if (resource.Value is ITexture texture)
					spriteBatch.SetTexture(PongVariables.MenuBackgroundSlot, texture);
			});

			pLoadCount = pLoadQueue.Count;

			SetupLoadSprites();
		}

		private void SetupLoadSprites()
		{
			var root = entityManager.CreateEntity("Root Loading");
			var rootTransform = root.CreateComponent<Transform2D>();
			pLoadingTransform = rootTransform;

			var spriteEntity = entityManager.CreateEntity("Loading Base");
			var spriteTransform = spriteEntity.CreateComponent<Transform2D>();
			spriteTransform.Scale = new Vector2(LoadingWidth + LoadingPaddingSize * 2, LoadingHeight + LoadingPaddingSize * 2);

			rootTransform.AddChild(spriteTransform);

			var sprite = spriteEntity.CreateComponent<SpriteComponent>();
			sprite.Color = Color.Black;

			spriteEntity = entityManager.CreateEntity("Loading Bar");
			pBarTransform = spriteEntity.CreateComponent<Transform2D>();
			pBarTransform.Position = new Vector2(LoadingPaddingSize);
			pBarTransform.Scale = new Vector2(0, LoadingHeight);

			rootTransform.AddChild(pBarTransform);

			sprite = spriteEntity.CreateComponent<SpriteComponent>();
			sprite.Color = Color.White;
		}

		public void OnUpdate()
		{
			renderState.DefaultClearColor = Color.White;
			if (pBarTransform is null || pLoadingTransform is null)
				return;

			// Center Sprite Bar
			var pos = mainWindow.Size.ToVector2() * 0.5f - new Vector2(LoadingWidth * 0.5f, LoadingHeight * 0.5f);
			pLoadingTransform.Position = pos;

			if (pLoadQueue.TryDequeue(out var action))
				action();
			else
			{
				// If queue items is empty, then we must go to next state
				gameStateManager.SetState(PongStates.PongMainMenuState);
				return;
			}

			var progress = (float)pLoadQueue.Count / pLoadCount;
			pBarTransform.Scale = new Vector2(progress * LoadingWidth, LoadingHeight);
		}

		public void OnExit()
		{
			renderState.DefaultClearColor = pDefaultClearColor;
			entityManager.DestroyAll();
		}

		private IAudio LoadAudio(string assetName, bool isStreamed = true)
		{
			BaseAudioAsset audioAsset;
			if (isStreamed)
				audioAsset = assetManager.GetAsset<StreamedAudioAsset>("Sounds/" + assetName);
			else
				audioAsset = assetManager.GetAsset<AudioAsset>("Sounds/" + assetName);

			if (audioAsset.Audio is null)
				throw new NullReferenceException($"Error has occurred while is loading {assetName}");

			return audioAsset.Audio;
		}

		private void LoadImage(string assetName, byte slotId)
		{
			var imageAsset = assetManager.GetAsset<ImageAsset>("Textures/"+assetName);
			var img = imageAsset.Image;
			spriteBatch.SetTexture(slotId, img);
		}

		private void LoadFont(string assetName)
		{
			var fontAsset = assetManager.GetAsset<FontAsset>(assetName);
			textRenderer.SetFont(fontAsset.Font);
		}
	}
}
