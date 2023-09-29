using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
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

		private readonly IServiceProvider pProvider;
		private readonly IEngine pEngine;
		private readonly RenderSettings pRenderSettings;
		private readonly ILogger<SpriteTextureManager> pLogger;

		private ITexture?[] pTextures;
		private IGraphicsDriver? pDriver;
		private LinkedList<Task<BuildResult>> pBuildTasks = new();

		private CancellationTokenSource pStopTasks = new CancellationTokenSource();
		private bool pDisposed = false;

		private Queue<(byte, ITexture?)> pTextures2Insert = new Queue<(byte, ITexture?)>();
		private Stopwatch pStopwatch = Stopwatch.StartNew();

		public ITexture?[] Textures { get => pTextures; }


		public bool IsReady
		{
			get => pBuildTasks.Count == 0 && pTextures2Insert.Count == 0;
		}

		public event EventHandler? OnUpdateTextures;
		
		public SpriteTextureManager(
			ILoggerFactory factory,
			IServiceProvider serviceProvider,
			RenderSettings renderSettings,
			IEngine engine,
			RPIEvents renderEvents)
		{
			pLogger = factory.Build<SpriteTextureManager>();
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
				var currTask = nextTask;
				if(!nextTask.Value.IsCompleted)
				{
					pLogger.Info("Task is not ready. Skipping!");
					nextTask = nextTask.Next;
					continue;
				}

				ProcessTask(currTask.Value);

				nextTask = currTask.Next;
				pBuildTasks.Remove(currTask);

				changed = true;
			}
		}
		private void ProcessTextures2Insert(ref bool changed)
		{
			(byte, ITexture?) item;
			while(pTextures2Insert.TryDequeue(out item))
			{
				(byte slot, ITexture? texture) = item;
				CheckAndSetTexture(slot, texture);
				changed = true;
			}
		}

		private void ProcessTask(Task<BuildResult> task)
		{
			BuildResult result = task.Result;
			pLogger.Info($"Set task texture on slot {result.Slot}");
			CheckAndSetTexture(result.Slot, result.Texture);
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			pStopTasks.Cancel();
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

			pTextures = Array.Empty<ITexture?>();
			pBuildTasks.Clear();

			pDisposed = true;
			pStopTasks.Dispose();
		}
		
		public void WaitTasks()
		{
			var nextTask = pBuildTasks.First;
			while(nextTask != null)
			{
				nextTask.Value.Wait();
				nextTask = nextTask.Next;
			}
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

		public void SetTextureNull(byte slot)
		{
			ITexture? tex = null;
			pTextures2Insert.Enqueue((slot, tex));
		}

		public void ClearTasks()
		{
			// we must wait for tasks and dispose created textures
			var tasks = pBuildTasks;
			pBuildTasks = new LinkedList<Task<BuildResult>>();

			pStopTasks.Cancel();
			pStopTasks = new CancellationTokenSource();

			Task.Run(() =>
			{
				var next = pBuildTasks.First;
				while(next != null)
				{
					try
					{
						next.Value.Wait();
						next.Value?.Result.Texture.Dispose();
					}
					catch { }
					next = next.Next;
				}
			});
		}

		public void ClearTextures()
		{
			for (byte i = 0; i < pTextures.Length; i++)
			{
				pTextures[i]?.Dispose();
				pTextures[i] = null;
			}
			ClearTasks();
			pTextures2Insert.Clear();
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
			throw new NotImplementedException("Method Broken");
			//if (pDriver is null)
			//	throw new NullReferenceException("Can´t build texture. Driver is required!");
			//pStopTasks.Token.ThrowIfCancellationRequested();
			//// Wait render finish before continue
			//if (pEngine.Step == EngineExecutionStep.Render)
			//{
			//	pLogger.Info("Waiting Render");
			//	await pEngine.WaitRender();
			//}
			//pStopTasks.Token.ThrowIfCancellationRequested();
			//pLogger.Info("Building Texture");
			//return pDriver.Device.CreateTexture(new TextureDesc
			//{
			//	Name = $"SpriteBatch Texture #{image.GetHashCode()}",
			//	Size = new TextureSize(image.Size.Width, image.Size.Height),
			//	Format = TextureFormat.RGBA8UNormSRGB,
			//	BindFlags = BindFlags.ShaderResource,
			//	Usage = Usage.Immutable
			//}, new ITextureData[]
			//{
			//	new ByteTextureData(image.Data, (ulong)(image.Size.Width * image.Components))
			//});
		}
	}
#endif
}
