using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;

namespace REngine.Core.Resources
{ 
	public abstract class BaseAssetManager(IServiceProvider serviceProvider) : IAssetManager
	{
		protected readonly Dictionary<ulong, Asset> mLoadedAssets = new();

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

		public virtual Asset? GetAsset(string assetName, Type assetType)
		{
			var assetHash = Hash.Digest(assetName);
			if (mLoadedAssets.TryGetValue(assetHash, out var asset))
			{

			}
			
			throw new NotImplementedException();
		}

		public T GetAsset<T>(string assetName) where T : Asset
		{
			throw new NotImplementedException();
		}
	}
}
