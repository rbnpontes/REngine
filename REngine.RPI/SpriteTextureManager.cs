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
		private bool pDirty = false;
		private bool pDisposed = false;

		private Stopwatch pStopwatch = Stopwatch.StartNew();

		public ITexture?[] Textures { get => pTextures; }
		public event EventHandler? OnUpdateTextures;
		
		public SpriteTextureManager(
			IServiceProvider serviceProvider,
			RenderSettings renderSettings,
			IEngine engine,
			EngineEvents engineEvents)
		{
			pProvider = serviceProvider;
			pRenderSettings = renderSettings;
			pTextures = new ITexture?[renderSettings.SpriteBatchMaxTextures];
			pEngine = engine;
			engineEvents.OnStart += HandleStart;
		}

		private void HandleStart(object? sender, EventArgs e)
		{
			pDriver = pProvider.Get<IGraphicsDriver>();
		}

		public void Update()
		{
			ProcessTasks();

			if (pDirty)
			{
				OnUpdateTextures?.Invoke(this, EventArgs.Empty);
				pDirty = false;
			}
		}

		private void ProcessTasks()
		{
			pStopwatch.Restart();

			var nextTask = pBuildTasks.First;
			while(nextTask != null && pStopwatch.ElapsedMilliseconds < pRenderSettings.SpriteBatchTexturesBuildTimeMs)
			{
				nextTask.Value.Wait();
				BuildResult result = nextTask.Value.Result;
				SetTexture(result.Slot, result.Texture);

				pBuildTasks.RemoveFirst();
				nextTask = pBuildTasks.First;
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
			if (pTextures[slot] == texture || slot == byte.MaxValue || pDisposed)
				return;

			pTextures[slot]?.Dispose();
			pTextures[slot] = texture;
			pDirty = true;
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

		private async Task<ITexture> BuildTexture(Image image)
		{
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
