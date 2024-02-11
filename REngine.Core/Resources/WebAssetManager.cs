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
        if(pStarted)
            return;
        var settings = mServiceProvider.Get<AssetManagerSettings>();
        if (settings.HttpSettings is null)
            throw new NullReferenceException(
                $"{nameof(HttpAssetManagerSettings)} is required on {nameof(AssetManagerSettings)}");
        Task.Run(async () =>
        {
            var data = await WebFetch.Get(settings.HttpSettings.MetadataUrl);
            var str = Encoding.UTF8.GetString(data);
            mLogger.Debug("Metadata: ", data);
        }).Wait();
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
    {
        throw new NotImplementedException();
    }
}