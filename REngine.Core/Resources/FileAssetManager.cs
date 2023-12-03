using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;

namespace REngine.Core.Resources
{
	public class FileAssetManager(IServiceProvider serviceProvider) : BaseAssetManager(serviceProvider)
	{
		private struct AssetEntry
		{
			public string Name;
			public string FilePath;
		}

		private readonly Dictionary<ulong, AssetEntry> pEntries = new();
		
		protected override void OnDispose()
		{
			pEntries.Clear();
		}

		protected override void OnStart()
		{
			var settings = mServiceProvider.Get<EngineSettings>();
			foreach (var searchPath in settings.AssetSearchPaths)
				WalkAndCollectFiles(searchPath, searchPath);
		}

		private void WalkAndCollectFiles(string rootPath, string path)
		{
			var directories = Directory.GetDirectories(path);
			var files = Directory.GetFiles(path);
			foreach (var file in files)
			{
				AssetEntry entry = new()
				{
					Name = file.Replace(rootPath, string.Empty),
					FilePath = file
				};
				pEntries[Hash.Digest(entry.Name)] = entry;
			}

			foreach (var dir in directories)
				WalkAndCollectFiles(rootPath, dir);
		}

		public override string[] GetAssets()
		{
			return pEntries.Values.Select(x => x.Name).ToArray();
		}

		public override AssetStream GetStream(string assetName)
		{
			var hash = Hash.Digest(assetName);
			if (!pEntries.TryGetValue(hash, out var entry))
				throw new NotFoundAssetException(assetName);
			var fileStream = new FileStream(entry.FilePath, FileMode.Open, FileAccess.Read);
			return new AssetStream(assetName, fileStream);
		}
	}
}
