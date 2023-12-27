using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.RHI;

namespace REngine.RPI.Resources;

public sealed class ShaderAsset(IShaderManager shaderManager, ILoggerFactory loggerFactory) : Asset
{
    private readonly IShaderManager pShaderManager = shaderManager;
    private readonly ILogger<ShaderAsset> pLogger = loggerFactory.Build<ShaderAsset>();
    public string ShaderCode { get; private set; } = string.Empty;
    
    protected override void OnLoad(AssetStream stream)
    {
        using var shaderStream = new StreamedShaderStream(stream);
        ShaderCode = shaderStream.GetShaderCode();
        pLogger.Debug($"Loaded Shader({Name}): ", ShaderCode);
    }


    public IShader BuildShader(ShaderType type)
    {
        return BuildShader(type, new Dictionary<string, string>());
    }

    public IShader BuildShader(ShaderType type, IDictionary<string, string> macros)
    {
        return shaderManager.GetOrCreate(new ShaderCreateInfo
        {
            Type = type,
            Name = Name,
            Macros = macros,
            SourceCode = ShaderCode
        });
    }
    
    protected override void OnDispose()
    {
    }
}