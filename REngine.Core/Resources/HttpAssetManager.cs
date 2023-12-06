using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Serialization;

namespace REngine.Core.Resources;

public sealed class HttpAssetManager(
    ILoggerFactory loggerFactory,
    EngineEvents engineEvents,
    IServiceProvider serviceProvider)
    : BaseAssetManager(loggerFactory, engineEvents, serviceProvider)
{
    private bool pStarted;
    private Dictionary<string, string> pAssetAddresses = new();
    
    protected override void OnStart()
    {
        if (pStarted)
            return;
        var settings = mServiceProvider.Get<AssetManagerSettings>();
        if (settings.HttpSettings is null)
            throw new NullReferenceException(
                $"{nameof(HttpAssetManagerSettings)} is required on {nameof(AssetManagerSettings)}.");

        Task.Run(async () =>
        {
            using var httpClient = new HttpClient();
            var res = await httpClient.GetAsync(settings.HttpSettings.MetadataUrl);
            if (!res.IsSuccessStatusCode)
                throw new HttpAssetMetadataDownload(settings.HttpSettings.MetadataUrl);
            var data = await res.Content.ReadAsStringAsync();
            var assetAddresses = (data ?? string.Empty).FromJson<Dictionary<string, string>>();
            if (assetAddresses is null)
                throw new HttpAssetMetadataParse(data);
        }).Wait();
        pStarted = true;
    }

    protected override void OnDispose()
    {
        pAssetAddresses.Clear();
    }

    public override string[] GetAssets()
    {
        return pAssetAddresses.Keys.ToArray();
    }

    public override AssetStream GetStream(string assetName)
    {
        OnStart();
        if (!pAssetAddresses.TryGetValue(assetName, out var assetUrl))
            throw new NotFoundAssetException(assetName);
        var downloadTask = DownloadFile(assetUrl);
        downloadTask.Wait();

        var memStream = new MemoryStream(downloadTask.Result);
        return new AssetStream(assetName, memStream);
    }

    private Task<byte[]> DownloadFile(string url)
    {
        return Task.Run(async () =>
        {
            using var httpClient = new HttpClient();
            var res = await httpClient.GetAsync(url);
            if (!res.IsSuccessStatusCode)
                throw new HttpAssetDownload(url);
            return await res.Content.ReadAsByteArrayAsync();
        });
    }
}