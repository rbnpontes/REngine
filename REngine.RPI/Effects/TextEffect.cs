using REngine.Core.Reflection;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RPI.Constants;
using REngine.RPI.Resources;

namespace REngine.RPI.Effects;

public abstract class BaseTextEffect : IDisposable
{
    private IPipelineState? pPipelineState;
    public bool IsDisposed { get; private set; }
    
    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        OnDispose();
    }

    public virtual IPipelineState BuildPipeline()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (pPipelineState is not null)
            return pPipelineState;
        return pPipelineState = OnBuildPipeline();
    }
    protected abstract void OnDispose();
    protected abstract IPipelineState OnBuildPipeline();
}

public class TextEffect(
    IShaderManager shaderManager,
    IPipelineStateManager pipelineStateManager,
    IAssetManager assetManager,
    GraphicsSettings settings
) : BaseTextEffect
{
    protected override void OnDispose()
    {
    }

    protected override IPipelineState OnBuildPipeline()
    {
        OnCreatePipelineDesc(out var pipelineDesc);
        
        var pipeline = pipelineStateManager.GetOrCreate(pipelineDesc);
        return pipeline;
    }

    protected virtual void OnCreatePipelineDesc(out GraphicsPipelineDesc desc)
    {
        desc = new GraphicsPipelineDesc()
        {
            Name = "Default Text Effect"
        };
        desc.Output.RenderTargetFormats[0] = settings.DefaultColorFormat;
        desc.Output.DepthStencilFormat = settings.DefaultDepthFormat;
        desc.BlendState.BlendMode = BlendMode.Alpha;
        desc.PrimitiveType = PrimitiveType.TriangleStrip;
        desc.RasterizerState.CullMode = CullMode.Both;
        desc.DepthStencilState.EnableDepth = false;
        desc.DepthStencilState.DepthWriteEnabled = false;

        desc.Shaders.VertexShader = OnGetShader(ShaderType.Vertex);
        desc.Shaders.PixelShader = OnGetShader(ShaderType.Pixel);

        for (var i = 0u; i < 2; ++i)
        {
            desc.InputLayouts.Add(
                new PipelineInputLayoutElementDesc()
                {
                    InputIndex = i,
                    Input = new InputLayoutElementDesc()
                    {
                        BufferIndex = 0,
                        ElementType = ElementType.Vector4,
                        InstanceStepRate = 1
                    }
                }    
            );
        }
        
        desc.Samplers.Add(
            new ImmutableSamplerDesc()
            {
                Name = TextureNames.MainTexture,
                Sampler = new SamplerStateDesc(TextureFilterMode.Anisotropic)
            }    
        );
    }

    protected virtual IShader OnGetShader(ShaderType shaderType)
    {
        OnGetShaderCreateInfo(shaderType, out var shaderCi);
        return shaderManager.GetOrCreate(shaderCi);
    }

    protected virtual void OnGetShaderCreateInfo(ShaderType shaderType, out ShaderCreateInfo shaderCi)
    {
        shaderCi = new ShaderCreateInfo()
        {
            Type = shaderType
        };
        
        string assetPath;
        switch (shaderType)
        {
            case ShaderType.Vertex:
                shaderCi.Name = $"[Vertex]{nameof(TextEffect)}";
                assetPath = "Shaders/text_vs.hlsl";
                break;
            case ShaderType.Pixel:
                shaderCi.Name = $"[Pixel]{nameof(TextEffect)}";
                assetPath = "Shaders/text_ps.hlsl";
                break;
            case ShaderType.Compute:
            case ShaderType.Geometry:
            case ShaderType.Hull:
            case ShaderType.Domain:
            default:
                throw new NotImplementedException();
        }

        shaderCi.SourceCode = assetManager.GetAsset<ShaderAsset>(assetPath).ShaderCode;
    }

    public static TextEffect Build(IServiceProvider provider)
    {
        return ActivatorExtended.CreateInstance<TextEffect>(provider) ?? throw new NullReferenceException();
    }
}