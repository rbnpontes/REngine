using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Resources
{
	public interface IAssetManager
	{
		/**
		 * Retrieve all Mapped Assets
		 * This does not mean that these assets are loaded.
		 * If you need to get loaded assets,
		 * Use <seealso cref="GetLoadedAssets"/> instead.
		 */
		public string[] GetAssets();
		/**
		 * Retrieve all Loaded Assets
		 * by the <see cref="AddAsset"/> method.
		 */
		public Asset[] GetLoadedAssets();
		/**
		 * Load Asset Data into <see cref="AssetStream"/>
		 */
		public AssetStream GetStream(string assetName);
		/**
		 * Add <see cref="Asset"/> to this manager
		 * this asset will be threat as Loaded Asset
		 */
		public IAssetManager AddAsset(Asset asset);
		/**
		 * Unload asset from this manager
		 */
		public IAssetManager UnloadAsset(Asset asset);
		/**
		 * Unload all assets
		 */
		public IAssetManager UnloadAssets();
		/**
		 * Get or Load asset.
		 * If asset is reachable, then a <see cref="Asset"/> object
		 * will return, otherwise a null object will return instead
		 */
		public Asset GetAsset(string assetName, Type assetType);
		public T GetAsset<T>(string assetName) where T : Asset;
		public void TryGetAsset(string assetName, Type assetType, out Asset? asset);
		public void TryGetAsset<T>(string assetName, out T? asset) where T : Asset;
	}
}
