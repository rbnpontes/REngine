using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.Core.Threading;
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
		private readonly IServiceProvider pProvider;
		private readonly RenderSettings pRenderSettings;
		private readonly ILogger<SpriteTextureManager> pLogger;

		private ITexture?[] pTextures;
		private IGraphicsDriver? pDriver;

		private bool pDisposed = false;
		private bool pDirtyTextures = false;

		private readonly LinkedList<(byte, ITexture?)> pTex2Insert = new();
		private readonly LinkedList<(byte, Image)> pTex2Build = new();
		private readonly object pTex2InsertSync = new();
		private readonly object pTex2BuildSync = new();

		private Stopwatch pStopwatch = Stopwatch.StartNew();

		public readonly object TextureSyncObj = new();
		public ITexture?[] Textures { get => pTextures; }
		public bool IsReady
		{
			get
			{
				int tex2InsertCount;
				int tex2BuildCount;

				lock (pTex2InsertSync)
					tex2InsertCount = pTex2Insert.Count;
				lock (pTex2BuildSync)
					tex2BuildCount = pTex2Build.Count;

				return tex2InsertCount == 0 && tex2BuildCount == 0;
			}
		}

		public event EventHandler? OnRebuildTextures;
		public event EventHandler? OnUpdateTextures;

		public SpriteTextureManager(
			ILoggerFactory factory,
			IServiceProvider serviceProvider,
			RenderSettings renderSettings)
		{
			pLogger = factory.Build<SpriteTextureManager>();
			pProvider = serviceProvider;
			pRenderSettings = renderSettings;
			pTextures = new ITexture?[renderSettings.SpriteBatchMaxTextures];
		}

		public void RecreateTextures()
		{
			if (pRenderSettings.SpriteBatchMaxTextures == pTextures.Length)
				return;

			pDirtyTextures = true;
		}

		public void Start()
		{
			pDriver = pProvider.Get<IGraphicsDriver>();
			IExecutionPipeline pipeline = pProvider.Get<IExecutionPipeline>();
			pipeline
				.AddEvent(DefaultEvents.SpriteBatchTaskId, (_) => VerifyResources())
				.AddEvent(DefaultEvents.SpriteBatchBuildTexId, (_) => BuildAndInsertTextures());
		}

		private void VerifyResources()
		{
			if (!pDirtyTextures)
				return;

			pLogger.Info("RenderSettings has been changed, recreating texture slots.");
			// Resize textures and move current
			var currTextures = pTextures;
			pTextures = new ITexture[pRenderSettings.SpriteBatchMaxTextures];
			Array.Copy(currTextures, pTextures, Math.Min(pTextures.Length, currTextures.Length));

			if (currTextures.Length > 0)
			{
				// Dispose textures that has not being added to the new array
				var diff = currTextures.Length - pTextures.Length;
				for (int i = 0; i < diff; ++i)
					currTextures[diff + i]?.Dispose();
			}

			OnRebuildTextures?.Invoke(this, EventArgs.Empty);
		}

		private void BuildAndInsertTextures()
		{
			lock (pTex2BuildSync)
				BuildTextures();
			lock (pTex2InsertSync)
				InsertTextures();
		}

		private void BuildTextures()
		{
			pStopwatch.Restart();

			var nextImg = pTex2Build.First;
			while (nextImg != null && pStopwatch.ElapsedMilliseconds < pRenderSettings.SpriteBatchTexturesBuildTimeMs)
			{
				(byte slot, Image image) = nextImg.Value;

				SetTexture(slot, BuildTexture(image));

				var oldImg = nextImg;
				nextImg = nextImg.Next;
				pTex2Build.Remove(oldImg);
			}
		}

		private void InsertTextures()
		{
			if (pTex2Insert.Count == 0)
				return;

			var nextTex = pTex2Insert.First;
			while (nextTex != null)
			{
				(byte slot, ITexture? tex) = nextTex.Value;
				pTextures[slot]?.Dispose();
				pTextures[slot] = tex;

				nextTex = nextTex.Next;
			}

			OnUpdateTextures?.Invoke(this, EventArgs.Empty);

			pTex2Insert.Clear();
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			lock (pTex2BuildSync)
				pTex2Build.Clear();
			lock (pTex2InsertSync)
			{
				var nextTex = pTex2Insert.First;
				while (nextTex != null)
				{
					nextTex.Value.Item2?.Dispose();
					nextTex = nextTex.Next;
				}
				pTex2Insert.Clear();
			}

			lock (TextureSyncObj)
			{
				foreach (var tex in pTextures)
					tex?.Dispose();
			}

			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		public void SetTexture(byte slot, ITexture texture)
		{
			if (slot == byte.MaxValue || slot >= pTextures.Length || pDisposed)
				return;

			lock (pTex2InsertSync)
				pTex2Insert.AddLast((slot, texture));
		}

		public void SetTexture(byte slot, Image image)
		{
			if (slot == byte.MaxValue || slot >= pTextures.Length || pDisposed)
				return;

			lock (pTex2BuildSync)
				pTex2Build.AddLast((slot, image));
		}

		public void SetTextureNull(byte slot)
		{
			if (slot == byte.MaxValue || slot >= pTextures.Length || pDisposed)
				return;
			lock (pTex2InsertSync)
				pTex2Insert.AddLast((slot, null));
		}

		public void ClearTasks()
		{
			lock (pTex2InsertSync)
				pTex2Insert.Clear();
			lock (pTex2Build)
				pTex2Insert.Clear();
		}

		public void ClearTextures()
		{
			ClearTasks();

			lock (TextureSyncObj)
			{
				for (byte i = 0; i < pTextures.Length; i++)
				{
					pTextures[i]?.Dispose();
					pTextures[i] = null;
				}
			}
		}

		private ITexture BuildTexture(Image image)
		{
			if (pDriver is null)
				throw new NullReferenceException("Can´t build texture. Driver is required!");

			pLogger.Info("Building Texture");
			return pDriver.Device.CreateTexture(new TextureDesc
			{
				Name = $"SpriteBatch Texture #{image.GetHashCode()}",
				Size = new TextureSize(image.Size.Width, image.Size.Height),
				Format = TextureFormat.RGBA8UNormSRGB,
				BindFlags = BindFlags.ShaderResource,
				Usage = Usage.Immutable
			}, new ITextureData[]
			{
				new ByteTextureData(image.Data, image.Stride)
			});
		}
	}
#endif
}