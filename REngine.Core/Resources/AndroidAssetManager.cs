#if ANDROID
using Android.Content.Res;
using REngine.Core.IO;

namespace REngine.Core.Resources;

public class AndroidAssetManager : BaseAssetManager
{
    private readonly List<string> pAssetEntries = [];
    private bool pStarted;
    // TODO: change this to a better approach
    public static AssetManager? PlatformAssetManager { get; set; }
    
    public AndroidAssetManager(ILoggerFactory loggerFactory, EngineEvents engineEvents, IServiceProvider serviceProvider) : base(loggerFactory, engineEvents, serviceProvider)
    {
    }
    protected override void OnStart()
    {
        if (pStarted)
            return;
        if (PlatformAssetManager is null)
            throw new NullReferenceException($"Android {nameof(AssetManager)} is required.");
        WalkAndCollectAssets(String.Empty, PlatformAssetManager);
        pStarted = true;
    }

    private void WalkAndCollectAssets(string path, AssetManager assetMgr)
    {
        var files = assetMgr.List(path) ?? [];
        var directories = new List<string>();
        
        foreach (var file in files)
        {
            var filePath = path + file;
            try
            {
                assetMgr.Open(filePath).Dispose();
                pAssetEntries.Add(filePath);
            }
            catch // If fails, probably is a directory
            {
                directories.Add(filePath);
            }
        }

        foreach (var dir in directories)    
            WalkAndCollectAssets(dir+"/", assetMgr);
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
        if (PlatformAssetManager is null)
            throw new NullReferenceException($"Platform {nameof(AssetManager)} must be set to get stream.");
        return new AssetStream(assetName, PlatformAssetManager.Open(assetName));
    }
}
#endif