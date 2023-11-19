using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.Core.WorldManagement;
using REngine.RPI;
using REngine.RPI.Components;

namespace REngine.Sandbox.Samples
{
	[Sample("Entity Component System")]
	internal class EntityComponentSystemSample : ISample
	{
		private readonly List<SpriteComponent> pComponents = new();
		private readonly Random pRandom = new();
		private readonly object pSync = new();

		private readonly string pSceneFilePath =
			Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Scenes/sprite_scene.rscene");

		private readonly Queue<SpriteComponent> pComponents2Remove = new();

		private bool pHasSceneFile;
		
		private EntityManager? pEntityManager;

		private IRenderer? pRenderer;
		private IImGuiSystem? pImGuiSystem;
		private IRenderFeature? pRenderFeature;
		private IServiceProvider? pServiceProvider;

		public IWindow? Window { get; set; }

		public EntityComponentSystemSample()
		{
			pHasSceneFile = File.Exists(pSceneFilePath);
		}

		public void Dispose()
		{
			pComponents.ForEach(x => x.Owner?.Dispose());

			pRenderer?.RemoveFeature(pRenderFeature);

			if(pImGuiSystem != null)
				pImGuiSystem.OnGui -= OnGui;
		}

		public void Load(IServiceProvider provider)
		{
			if (Window is null)
				return;

			pServiceProvider = provider;
			pRenderer = provider.Get<IRenderer>();
			pEntityManager = provider.Get<EntityManager>();
			var spriteBatch = provider.Get<ISpriteBatch>();

			// Load Sprite
			ImageAsset sprite = new("doge.png");
			using (FileStream stream = new(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Textures/doge.jpg"), FileMode.Open))
				sprite.Load(stream).Wait();

			spriteBatch.SetTexture(0, sprite.Image);

			pRenderer = provider.Get<IRenderer>();
			// To sprite component work, we must add sprite batch render feature
			pRenderer.AddFeature(pRenderFeature = spriteBatch.Feature);

			pImGuiSystem = provider.Get<IImGuiSystem>();
			pImGuiSystem.OnGui += OnGui;

			var wndSize = Window.Size;
			InstantiateSpriteComponent(
				new Vector2(
					(wndSize.Width * 0.5f) - 50,
					(wndSize.Height * 0.5f) - 50
				),
		0f
			);
		}

		private void InstantiateSpriteComponent(Vector2 position, float rotation)
		{
			if (pServiceProvider is null || pEntityManager is null)
				return;
			lock (pSync)
			{
				var entity = pEntityManager.CreateEntity($"Sprite Component #{pComponents.Count}");

				var spriteComponent = new SpriteComponent(pServiceProvider);

				entity.AddComponent(spriteComponent);

				spriteComponent.Transform.Position = position;
				spriteComponent.Transform.Rotation = rotation;

				spriteComponent.Transform.Scale = new Vector2(100, 100);

				spriteComponent.TextureSlot = 0;
				pComponents.Add(spriteComponent);
			}
		}

		private void RemoveSpriteComponent(SpriteComponent spriteComponent)
		{
			pComponents.Remove(spriteComponent);
			spriteComponent.Dispose();
		}

		private void OnGui(object? sender, EventArgs e)
		{
			ImGui.Begin("Entity Component System Sample");

			RenderSpriteItems();
			ImGui.Separator();
			RenderCreateNewSpriteButton();
			ImGui.Separator();
			RenderEcsSerializerButtons();
			ImGui.End();
		}

		private void RenderSpriteItems()
		{
			// ImGui runs on separated thread
			lock (pSync)
			{
				var components = pComponents.Where(x => x.Owner is not null)
					.Where(x => ImGui.CollapsingHeader((x.Owner?.Name ?? string.Empty) + $" - #EntityID: {x.Owner?.Id ?? -1}"));
				foreach (var component in components)
					RenderSpriteProperties(component);

				while(pComponents2Remove.TryDequeue(out var component))
					RemoveSpriteComponent(component);
			}
		}
		private void RenderSpriteProperties(SpriteComponent spriteComponent)
		{
			var enabled = spriteComponent.Owner?.Enabled ?? true;
			var position = spriteComponent.Transform.Position;
			var rotation = spriteComponent.Transform.Rotation;
			var scale = spriteComponent.Transform.Scale;
			var color = spriteComponent.Color.ToVector4();

			ImGui.Checkbox("Enabled", ref enabled);
			ImGui.DragFloat2("Position", ref position);
			ImGui.DragFloat("Rotation", ref rotation, 0.01f);
			ImGui.DragFloat2("Scale", ref scale);
			ImGui.ColorPicker4("Color", ref color);

			if (spriteComponent.Owner != null) spriteComponent.Owner.Enabled = enabled;
			spriteComponent.Transform.Position = position;
			spriteComponent.Transform.Rotation = rotation;
			spriteComponent.Transform.Scale = scale;
			spriteComponent.Color = color.ToColor();

			if(ImGui.Button("Remove Component"))
				pComponents2Remove.Enqueue(spriteComponent);
		}

		private void RenderCreateNewSpriteButton()
		{
			if (Window is null)
				return;

			if (!ImGui.Button("Create New Sprite"))
				return;

			var wndSize = Window.Size;

			var x = (float)pRandom.NextDouble() * wndSize.Width;
			var y = (float)pRandom.NextDouble() * wndSize.Height;
			var rotation = (float)pRandom.NextDouble();

			InstantiateSpriteComponent(new Vector2(x, y), rotation);
		}

		private void RenderEcsSerializerButtons()
		{
			if (pHasSceneFile)
			{
				if(ImGui.Button("Load Scene 'sprite_scene.rscene'"))
					LoadScene();
			}

			if(ImGui.Button("Save Scene 'sprite_scene.rscene'"))
				SaveScene();
		}

		private void LoadScene()
		{
			lock (pSync)
			{
				pEntityManager?.DestroyAll();
				pEntityManager?.Load(pSceneFilePath);
				pComponents.Clear();

				var entities = pEntityManager?.GetEntities() ?? Array.Empty<Entity>();
				foreach (var entity in entities)
				{
					var component = entity.GetComponent<SpriteComponent>();
					if(component != null)
						pComponents.Add(component);
				}
			}
		}

		private void SaveScene()
		{
			if (!pHasSceneFile)
			{
				var dir = Path.GetDirectoryName(pSceneFilePath) ?? string.Empty;
				if(!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}

			lock (pSync)
			{
				pEntityManager?.Save(pSceneFilePath);
				if(!pHasSceneFile)
					pHasSceneFile = true;
			}
		}

		public void Update(IServiceProvider provider)
		{
		}
	}
}
