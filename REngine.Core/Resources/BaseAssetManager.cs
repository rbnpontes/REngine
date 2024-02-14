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

		public virtual async Task<AssetStream> GetAsyncStream(string assetName)
		{
			await Task.Yield();
			return GetStream(assetName);
		}
		
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
			GC.Collect();
			GC.WaitForPendingFinalizers();
			return this;
		}
		
		private static void LogAssetNotFound(ILogger<IAssetManager> logger, string assetName)
		{
			logger.Error($"Asset '{assetName}' not found.");
		}

		protected bool TryFindAsset(ref string assetName, Type assetType, out Asset? asset, out ulong assetHash)
		{
			assetName = NormalizeAssetName(assetName);
			assetHash = Hash.Digest(assetName);
#if DEBUG
			if (!assetType.IsSubclassOf(typeof(Asset)))
				throw new ArgumentException($"Asset Type must inherit {nameof(Asset)}");
#endif
			if (!mLoadedAssets.TryGetValue(assetHash, out asset))
			{
				LogAssetNotFound(mLogger, assetName);
				return false;
			}
			
			var result = asset.GetType().IsAssignableTo(assetType);
			if (!result)
				LogAssetNotFound(mLogger, assetName);
			return result;
		}

		private void TryBuildAsset(AssetStream stream, string assetName, ulong assetHash, Type assetType, out Asset? asset)
		{
			asset = ActivatorExtended.CreateInstance(mServiceProvider, assetType) as Asset;
			if (asset is null)
			{
				LogAssetNotFound(mLogger, assetName);
				return;
			}
			
			mLogger.Info($"Loading Asset: {assetName}");
#if DEBUG
			var profileKey = $"Load {assetName} Time";
			mLogger.Profile(profileKey);
#endif
			asset.Load(stream);
#if DEBUG
			mLogger.EndProfile(profileKey);
#endif
			mLogger.Success($"Loaded Asset: {assetName}");
			mLoadedAssets[assetHash] = asset;
		}
		
		public void TryGetAsset(string assetName, Type assetType, out Asset? asset)
		{
			if (TryFindAsset(ref assetName, assetType, out asset, out var assetHash))
				return;

			var stream = GetStream(assetName);
			TryBuildAsset(stream, assetName, assetHash, assetType, out asset);
		}

		public void TryGetAsset<T>(string assetName, out T? asset) where T : Asset
		{
			TryGetAsset(assetName, typeof(T), out var tmpAsset);
			asset = (T?)tmpAsset;
		}

		public virtual async Task<Asset?> TryGetAsyncAsset(string assetName, Type assetType)
		{
			if (TryFindAsset(ref assetName, assetType, out var asset, out var assetHash))
				return asset;

			var stream = await GetAsyncStream(assetName);
			TryBuildAsset(stream, assetName, assetHash, assetType, out asset);
			return asset;
		}

		public virtual async Task<T?> TryGetAsyncAsset<T>(string assetName) where T : Asset
		{
			var asset = await TryGetAsyncAsset(assetName, typeof(T));
			return (T?)asset;
		}
		
		public virtual Asset GetAsset(string assetName, Type assetType)
		{
			TryGetAsset(assetName, assetType, out var asset);
			return asset ?? throw new NotFoundAssetException(assetName);
		}

		public virtual async Task<Asset> GetAsyncAsset(string assetName, Type assetType)
		{
			await Task.Yield();
			return GetAsset(assetName, assetType);
		}

		public T GetAsset<T>(string assetName) where T : Asset
		{
			TryGetAsset<T>(assetName, out var asset);
			return asset ?? throw new NotFoundAssetException(assetName);
		}

		public virtual async Task<T> GetAsyncAsset<T>(string assetName) where T : Asset
		{
			await Task.Yield();
			return GetAsset<T>(assetName);
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
