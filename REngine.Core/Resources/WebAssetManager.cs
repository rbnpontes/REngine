using System.Text;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.Core.Web;

namespace REngine.Core.Resources;

public sealed class WebAssetManager(
    ILoggerFactory loggerFactory, 
    EngineEvents engineEvents, 
    IServiceProvider serviceProvider) : BaseAssetManager(loggerFactory, engineEvents, serviceProvider)
{
    private bool pStarted;
    private Dictionary<string, string> pAssetAddresses = new();
    protected override void OnStart()
    {
        mLogger.Warning($"Invalid Call. Please use '{nameof(OnStartAsync)}' instead");
    }

    private async Task OnStartAsync()
    {
        if (pStarted)
            return;
        var settings = mServiceProvider.Get<AssetManagerSettings>();
        if (settings.HttpSettings is null)
            throw new NullReferenceException(
                $"{nameof(HttpAssetManagerSettings)} is required on {nameof(AssetManagerSettings)}");
        mLogger.Debug("Loading Metadata: " + settings.HttpSettings.MetadataUrl);
        var data = await WebFetch.Get(settings.HttpSettings.MetadataUrl);
        var items = Encoding.UTF8.GetString(data).Split('\n');
        var baseAssetPath = settings.HttpSettings.MetadataUrl.Replace(Path.GetFileName(settings.HttpSettings.MetadataUrl), string.Empty);

        foreach (var item in items)
        {
            var targetItem = item.Trim();
            var assetPath = targetItem;
            if(string.IsNullOrEmpty(targetItem))
                continue;
            if (!(targetItem.StartsWith("http://") || targetItem.StartsWith("https://")))
                assetPath = $"{baseAssetPath}{item}";
            
            Console.WriteLine($"Asset Item: [{targetItem}] = {assetPath}");
            pAssetAddresses[targetItem] = assetPath;
        }
        pStarted = true;
    }

    protected override void OnDispose()
    {
    }

    public override string[] GetAssets()
    {
        return pAssetAddresses.Keys.ToArray();
    }

    public override AssetStream GetStream(string assetName)
        => throw new NotSupportedException(
            $"'{nameof(GetStream)}' is not supported on this Platform. Please use '{nameof(GetAsyncStream)}' instead.");

    public override async Task<AssetStream> GetAsyncStream(string assetName)
    {
        await OnStartAsync();
        if (!pAssetAddresses.TryGetValue(assetName, out var assetUrl))
            throw new NotFoundAssetException(assetName);
        var memStream = await DownloadFile(assetUrl);
        return new AssetStream(assetName, memStream);
    }

    public override Asset GetAsset(string assetName, Type assetType)
    {
        TryFindAsset(ref assetName, assetType, out var asset, out var assetHash);
        if (asset is not null)
            return asset;
        throw new NotFoundAssetException($"Not found preloaded asset at given path: '{assetName}'. To load asset from remote, try '{nameof(GetAsyncAsset)}' instead.");
    }
    
    public override async Task<Asset> GetAsyncAsset(string assetName, Type assetType)
    {
        var asset = await TryGetAsyncAsset(assetName, assetType);
        return asset ?? throw new NotFoundAssetException(assetName);
    }
    public override async Task<T> GetAsyncAsset<T>(string assetName)
    {
        var asset = await TryGetAsyncAsset<T>(assetName);
        return asset ?? throw new NotFoundAssetException(assetName);
    }

    private async Task<Stream> DownloadFile(string url)
    {
        var data = await WebFetch.Get(url);
        return new MemoryStream(data);
    }
}