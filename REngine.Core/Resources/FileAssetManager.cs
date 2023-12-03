using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Mathematics;

namespace REngine.Core.Resources
{
	public class FileAssetManager(
		ILoggerFactory loggerFactory,
		EngineEvents engineEvents,
		IServiceProvider serviceProvider) : BaseAssetManager(loggerFactory, engineEvents, serviceProvider)
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
				var name = NormalizeAssetName(file.Replace(rootPath, string.Empty));
				var hash = Hash.Digest(name);
				AssetEntry entry = new()
				{
					Name = name,
					FilePath = file
				};
				pEntries[hash] = entry;
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
			var hash = Hash.Digest(NormalizeAssetName(assetName));
			if (!pEntries.TryGetValue(hash, out var entry))
				throw new NotFoundAssetException(assetName);
			var fileStream = new FileStream(entry.FilePath, FileMode.Open, FileAccess.Read);
			return new AssetStream(assetName, fileStream);
		}
	}
}
