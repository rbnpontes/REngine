using REngine.Core.DependencyInjection;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RPI;
using REngine.RPI.Resources;

namespace REngine.Sandbox.PongGame.Effects;

public sealed class LogoEffect(IServiceProvider provider) : TextureSpriteEffect(
    provider.Get<IAssetManager>(),
    provider.Get<IPipelineStateManager>(),
    provider.Get<GraphicsSettings>(),
    provider.Get<IShaderResourceBindingCache>(),
    provider.Get<IBufferManager>(),
    provider.Get<IShaderManager>()
)
{
    private readonly IAssetManager pAssetManager = provider.Get<IAssetManager>();
    protected override void OnGetShaderCreateInfo(ShaderType shaderType, out ShaderCreateInfo shaderCi)
    {
        base.OnGetShaderCreateInfo(shaderType, out shaderCi);
        if (shaderType != ShaderType.Pixel) return;
        
        shaderCi.Name = "[Pixel] Logo Effect";
        shaderCi.SourceCode = pAssetManager.GetAsset<ShaderAsset>("Shaders/engine_logo_effect.hlsl").ShaderCode;
    }
}