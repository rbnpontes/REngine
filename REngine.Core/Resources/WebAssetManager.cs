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
        var data = await WebFetch.Get(settings.HttpSettings.MetadataUrl);
        var str = Encoding.UTF8.GetString(data);
        mLogger.Debug("Metadata: ", str);

        pStarted = true;
    }

    protected override void OnDispose()
    {
    }

    public override string[] GetAssets()
    {
        return [];
    }

    public override AssetStream GetStream(string assetName)
        => throw new NotSupportedException(
            $"'{nameof(GetStream)}' is not supported on this Platform. Please use '{nameof(GetAsyncStream)}' instead.");

    public override async Task<AssetStream> GetAsyncStream(string assetName)
    {
        await OnStartAsync();
        throw new NotImplementedException();
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
}