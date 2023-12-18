#if ANDROID
using Android.Content.Res;
using REngine.Core.IO;
using REngine.Core.Resources;

namespace REngine.Core.Android;

public class AndroidAssetManager(
    AssetManager assetManager,
    ILoggerFactory loggerFactory,
    EngineEvents engineEvents,
    IServiceProvider serviceProvider)
    : BaseAssetManager(loggerFactory, engineEvents, serviceProvider)
{
    private readonly List<string> pAssetEntries = [];
    private bool pStarted;

    protected override void OnStart()
    {
        if (pStarted)
            return;
        WalkAndCollectAssets(string.Empty);
        pStarted = true;
    }

    private void WalkAndCollectAssets(string path)
    {
        var files = assetManager.List(path) ?? [];
        var directories = new List<string>();
        
        foreach (var file in files)
        {
            var filePath = path + file;
            try
            {
                assetManager.Open(filePath).Dispose();
                pAssetEntries.Add(filePath);
            }
            catch // If fails, probably is a directory
            {
                directories.Add(filePath);
            }
        }

        foreach (var dir in directories)    
            WalkAndCollectAssets(dir+"/");
    }

    protected override void OnDispose()
    {
        pAssetEntries.Clear();
    }

    public override string[] GetAssets()
    {
        return pAssetEntries.ToArray();
    }

    public override AssetStream GetStream(string assetName)
    {
        if (!pStarted)
            OnStart();
        var stream = assetManager.Open(assetName, Access.Buffer);
        // Copy AssetManager Stream to MemoryStream
        // This is required because AssetManager does not 
        // provide Stream Length on Compressed Data, this causes
        // Exception when IAssetManager tries to Load an asset.
        var memStream = new MemoryStream();
        stream.CopyTo(memStream);
        stream.Dispose();
        // When copy occurs, position is advanced to data length
        // We must reset to 0, otherwise Read operations will not work
        memStream.Position = 0;
        return new AssetStream(assetName, memStream);
    }
}
#endif