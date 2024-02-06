using REngine.Core.IO;
using REngine.Core.Reflection;
using REngine.Core.Serialization;
using REngine.Core.Web.IO;
using REngine.RHI;
using REngine.RPI;

namespace REngine.Core.Web;

public sealed class WebEngineInstance(IEngineApplication app, IWebStorage storage) : EngineInstance(app)
{
    private readonly GraphicsSettings pGraphicsSettings = new();
    private readonly RenderSettings pRenderSettings = new();
    protected override ILoggerFactory OnGetLoggerFactory()
    {
        return new WebLoggerFactory();
    }

    protected override void OnWriteSettings()
    {
         base.OnWriteSettings();
         WriteSettings(EngineSettings.GraphicsSettingsPath, pGraphicsSettings);
         WriteSettings(EngineSettings.RenderSettingsPath, pRenderSettings);
    }

    protected override void WriteSettings<T>(string path, T data)
    {
        storage.SetItem($"@rengine/{path}", data.ToJson());
    }

    protected override T LoadSettings<T>(string path)
    {
        var value = storage.GetItem($"@rengine/{path}");
        if (string.IsNullOrEmpty(value))
            return ActivatorExtended.CreateInstance<T>([]);
        return value.FromJson<T>() ?? ActivatorExtended.CreateInstance<T>([]);
    }
}