using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.RHI;

namespace REngine.RPI.Resources;

public sealed class ShaderAsset(IShaderManager shaderManager) : Asset
{
    private readonly IShaderManager pShaderManager = shaderManager;

    public string ShaderCode { get; private set; } = string.Empty;

    protected override void OnLoad(AssetStream stream)
    {
        using var shaderStream = new StreamedShaderStream(stream);
        ShaderCode = shaderStream.GetShaderCode();
    }

    protected override void OnDispose()
    {
    }
}