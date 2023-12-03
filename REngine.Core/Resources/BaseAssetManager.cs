using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Events;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Reflection;

namespace REngine.Core.Resources
{ 
	public abstract class BaseAssetManager : IAssetManager, IDisposable
	{
		private readonly EngineEvents pEngineEvents;
		protected readonly IServiceProvider mServiceProvider;
		protected readonly Dictionary<ulong, Asset> mLoadedAssets = new();
		protected readonly ILogger<IAssetManager> mLogger;
		
		private bool pDisposed;
		protected BaseAssetManager(
			ILoggerFactory loggerFactory,
			EngineEvents engineEvents,
			IServiceProvider serviceProvider)
		{
			mServiceProvider = serviceProvider;
			mLogger = loggerFactory.Build<IAssetManager>();
			pEngineEvents = engineEvents;
			pEngineEvents.OnStop +=	HandleEngineStop;
			pEngineEvents.OnStart += HandleEngineStart;
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			pEngineEvents.OnStart -= HandleEngineStart;
			mLogger.Profile("Start Time");
			OnStart();
			mLogger.EndProfile("Start Time");
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			pEngineEvents.OnStop -= HandleEngineStop;
			Dispose();
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			OnDispose();
			UnloadAssets();
			
			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		protected abstract void OnStart();
		protected abstract void OnDispose();
		public abstract string[] GetAssets();

		public virtual Asset[] GetLoadedAssets()
		{
			return mLoadedAssets.Values.ToArray();
		}

		public abstract AssetStream GetStream(string assetName);

		public virtual IAssetManager AddAsset(Asset asset)
		{
			mLoadedAssets[asset.ToHash()] = asset;
			return this;
		}

		public virtual IAssetManager UnloadAsset(Asset asset)
		{
			mLoadedAssets.Remove(asset.ToHash());
			asset.Dispose();
			return this;
		}

		public virtual IAssetManager UnloadAssets()
		{
			foreach(var asset in mLoadedAssets.Values)
				asset.Dispose();
			mLoadedAssets.Clear();
			return this;
		}

		public void TryGetAsset(string assetName, Type assetType, out Asset? asset)
		{
			assetName = NormalizeAssetName(assetName);
			var assetHash = Hash.Digest(assetName);
#if DEBUG
			if (!assetType.IsSubclassOf(typeof(Asset)))
				throw new ArgumentException($"Asset Type must inherit {nameof(Asset)}");
#endif

			if (mLoadedAssets.TryGetValue(assetHash, out asset))
			{
				if (!asset.GetType().IsAssignableTo(assetType))
					LogAssetNotFound();
				return;
			}

			var stream = GetStream(assetName);
			asset = ActivatorExtended.CreateInstance(mServiceProvider, assetType) as Asset;
			if (asset is null)
			{
				LogAssetNotFound();
				return;
			}

			mLogger.Info($"Loading Asset: {assetName}");
#if DEBUG
			var profileKey = $"Load {assetName} Time";
			mLogger.Profile(profileKey);
#endif
			asset.Load(stream).Wait();
#if DEBUG
			mLogger.EndProfile(profileKey);
#endif
			mLogger.Success($"Loaded Asset: {assetName}");
			mLoadedAssets[assetHash] = asset;
			return;

			void LogAssetNotFound()
			{
				mLogger.Error($"Asset '{assetName}' not found.");
			}
		}

		public void TryGetAsset<T>(string assetName, out T? asset) where T : Asset
		{
			TryGetAsset(assetName, typeof(T), out var tmpAsset);
			asset = (T?)tmpAsset;
		}
		public virtual Asset GetAsset(string assetName, Type assetType)
		{
			TryGetAsset(assetName, assetType, out var asset);
			return asset ?? throw new NotFoundAssetException(assetName);
		}

		public T GetAsset<T>(string assetName) where T : Asset
		{
			TryGetAsset<T>(assetName, out var asset);
			return asset ?? throw new NotFoundAssetException(assetName);
		}

		private static string NormalizePath(string path)
		{
			return path.Replace('\\', '/');
		}

		protected static string NormalizeAssetName(string path)
		{
			if (path.StartsWith('\\') || path.StartsWith('/'))
				path = path[1..];
			return NormalizePath(path);
		}
	}
}
