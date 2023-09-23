using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Resources;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
#if RENGINE_SPRITEBATCH
	internal class SpriteTextureManager : IDisposable
	{
		class BuildResult
		{
			public ITexture Texture;
			public byte Slot;
			public BuildResult(byte slot, ITexture texture)
			{
				Slot = slot;
				Texture = texture;
			}
		}

		private IServiceProvider pProvider;
		private ITexture?[] pTextures;
		private RenderSettings pRenderSettings;
		private IGraphicsDriver? pDriver;
		private IEngine pEngine;
		private LinkedList<Task<BuildResult>> pBuildTasks = new LinkedList<Task<BuildResult>>();

		private CancellationTokenSource pDisposeToken = new CancellationTokenSource();
		private bool pDisposed = false;

		private Queue<(byte, ITexture)> pTextures2Insert = new Queue<(byte, ITexture)>();
		private Stopwatch pStopwatch = Stopwatch.StartNew();

		public ITexture?[] Textures { get => pTextures; }

		public event EventHandler? OnUpdateTextures;
		
		public SpriteTextureManager(
			IServiceProvider serviceProvider,
			RenderSettings renderSettings,
			IEngine engine,
			RenderEvents renderEvents)
		{
			pProvider = serviceProvider;
			pRenderSettings = renderSettings;
			pTextures = new ITexture?[renderSettings.SpriteBatchMaxTextures];
			pEngine = engine;

			renderEvents.OnUpdateSettings += HandleUpdateSettings;
		}

		private void HandleUpdateSettings(object? sender, RenderUpdateSettingsEventArgs e)
		{
			if (e.Settings.SpriteBatchMaxTextures == pTextures.Length)
				return;

			// Resize textures and move current
			var currTextures = pTextures;
			pTextures = new ITexture[e.Settings.SpriteBatchMaxTextures];
			Array.Copy(currTextures, pTextures, Math.Min(pTextures.Length, currTextures.Length));

			if(currTextures.Length > 0)
			{
				// Dispose textures that has not being added to the new array
				var diff = currTextures.Length - pTextures.Length;
				for(int i =0; i < diff; ++i)
					currTextures[diff + i]?.Dispose();
			}

			OnUpdateTextures?.Invoke(this, EventArgs.Empty);
		}

		public void Start()
		{
			pDriver = pProvider.Get<IGraphicsDriver>();
		}

		public void Update()
		{
			bool changed = false;

			ProcessTasks(ref changed);
			ProcessTextures2Insert(ref changed);

			if (changed)
				OnUpdateTextures?.Invoke(this, EventArgs.Empty);
		}

		private void ProcessTasks(ref bool changed)
		{
			pStopwatch.Restart();
			var nextTask = pBuildTasks.First;

			while(nextTask != null && pStopwatch.ElapsedMilliseconds < pRenderSettings.SpriteBatchTexturesBuildTimeMs)
			{
				nextTask.Value.Wait();
				BuildResult result = nextTask.Value.Result;
				CheckAndSetTexture(result.Slot, result.Texture);

				pBuildTasks.RemoveFirst();
				nextTask = pBuildTasks.First;
				changed = true;
			}
		}
		private void ProcessTextures2Insert(ref bool changed)
		{
			(byte, ITexture) item;
			while(pTextures2Insert.TryDequeue(out item))
			{
				(byte slot, ITexture texture) = item;
				CheckAndSetTexture(slot, texture);
				changed = true;
			}
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			pDisposeToken.Cancel();
			// Wait tasks and dispose created textures
			var nextTask = pBuildTasks.First;
			while(nextTask != null)
			{
				try
				{
					nextTask.Value.Wait();
					var result = nextTask.Value.Result;
					result.Texture.Dispose();
					nextTask = nextTask.Next;
				}
				catch { }
			}

			foreach(var tex in pTextures)
				tex?.Dispose();

			pTextures = new ITexture?[0];
			pBuildTasks.Clear();

			pDisposed = true;
			pDisposeToken.Dispose();
		}
		
		public void SetTexture(byte slot, ITexture texture)
		{
			pTextures2Insert.Enqueue((slot, texture));
		}

		public void SetTexture(byte slot, Image image)
		{
			if (slot == byte.MaxValue || pDisposed)
				return;

			Task<BuildResult> task = Task.Run(async () =>
			{
				return new BuildResult(
					slot,
					await BuildTexture(image)
				);
			});
			pBuildTasks.AddLast(task);
		}

		private void CheckAndSetTexture(byte slot, ITexture texture)
		{
			if (pTextures[slot] == texture || slot == byte.MaxValue || pDisposed)
				return;

			pTextures[slot]?.Dispose();
			pTextures[slot] = texture;
		}

		private async Task<ITexture> BuildTexture(Image image)
		{
			if (pDriver is null)
				throw new NullReferenceException("Can´t build texture. Driver is required!");
			pDisposeToken.Token.ThrowIfCancellationRequested();
			// Wait render finish before continue
			if (pEngine.Step == EngineExecutionStep.Render)
				await pEngine.WaitRender();
			pDisposeToken.Token.ThrowIfCancellationRequested();
			return pDriver.Device.CreateTexture(new TextureDesc
			{
				Name = $"SpriteBatch Texture #{image.GetHashCode()}",
				Size = new TextureSize(image.Size.Width, image.Size.Height),
				Format = TextureFormat.RGBA8UNormSRGB,
				BindFlags = BindFlags.ShaderResource,
				Usage = Usage.Immutable
			}, new ITextureData[]
			{
				new ByteTextureData(image.Data, (ulong)(image.Size.Width * image.Components))
			});
		}
	}
#endif
}
