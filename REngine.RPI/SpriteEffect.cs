using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Reflection;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RPI.Constants;
using REngine.RPI.Resources;

namespace REngine.RPI
{
    public abstract class BaseSpriteEffect : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            OnDispose();
        }

        protected abstract void OnDispose();
        public abstract IPipelineState OnBuildPipeline();
        public abstract IShaderResourceBinding OnGetShaderResourceBinding();

        /// <summary>
        /// Called by the batch, use when you need to update internal buffers
        /// </summary>
        public virtual void UpdateBuffers()
        {
        }
    }

    public abstract class DefaultSpriteEffect(
        IPipelineStateManager pipelineStateManager,
        GraphicsSettings settings,
        IShaderResourceBindingCache shaderResourceBindingCache,
        IBufferManager bufferManager,
        IShaderManager shaderManager
    ) : BaseSpriteEffect
    {
        private IPipelineState? pPipelineState;
        private IShaderResourceBinding? pShaderResourceBinding;

        protected bool mDirtySrb = true;

        protected override void OnDispose()
        {
            pShaderResourceBinding?.Dispose();
            pShaderResourceBinding = null;
        }

        public override IPipelineState OnBuildPipeline()
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (pPipelineState is not null)
                return pPipelineState;
            pPipelineState = BuildPipeline();
            return pPipelineState;
        }

        protected virtual IPipelineState BuildPipeline()
        {
            OnCreatePipelineDesc(out var desc);
            return pipelineStateManager.GetOrCreate(desc);
        }

        protected virtual void OnCreatePipelineDesc(out GraphicsPipelineDesc desc)
        {
            desc = new GraphicsPipelineDesc
            {
                Name = "Default Sprite Effect"
            };
            desc.Output.RenderTargetFormats[0] = settings.DefaultColorFormat;
            desc.Output.DepthStencilFormat = settings.DefaultDepthFormat;
            desc.BlendState.BlendMode = BlendMode.Replace;
            desc.PrimitiveType = PrimitiveType.TriangleStrip;
            desc.RasterizerState.CullMode = CullMode.Both;
            desc.DepthStencilState.EnableDepth = false;

            desc.Shaders.VertexShader = OnGetShader(ShaderType.Vertex);
            desc.Shaders.PixelShader = OnGetShader(ShaderType.Pixel);
        }

        protected virtual IShader OnGetShader(ShaderType shaderType)
        {
            OnGetShaderCreateInfo(shaderType, out var shaderCi);
            return shaderManager.GetOrCreate(shaderCi);
        }

        protected abstract void OnGetShaderCreateInfo(ShaderType shaderType, out ShaderCreateInfo shaderCi);

        public override IShaderResourceBinding OnGetShaderResourceBinding()
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (!mDirtySrb && pShaderResourceBinding is not null) return pShaderResourceBinding;

            var resMapping = OnGetResourceMapping();
            var srb = shaderResourceBindingCache.Build(OnBuildPipeline(), resMapping);
            pShaderResourceBinding?.Dispose();
            pShaderResourceBinding = srb;
            return srb;
        }

        protected virtual ResourceMapping OnGetResourceMapping()
        {
            var resourceMapping = new ResourceMapping();
            resourceMapping
                .Add(ShaderTypeFlags.Vertex, ConstantBufferNames.Frame, bufferManager.GetBuffer(BufferGroupType.Frame))
                .Add(ShaderTypeFlags.Vertex, ConstantBufferNames.Object,
                    bufferManager.GetBuffer(BufferGroupType.Object));
            return resourceMapping;
        }
    }

    public class SpriteEffect(
        IAssetManager assetManager,
        IPipelineStateManager pipelineStateManager,
        GraphicsSettings settings,
        IShaderResourceBindingCache shaderResourceBindingCache,
        IBufferManager bufferManager,
        IShaderManager shaderManager
    ) : DefaultSpriteEffect(
        pipelineStateManager,
        settings,
        shaderResourceBindingCache,
        bufferManager,
        shaderManager
    )
    {
        protected override void OnGetShaderCreateInfo(ShaderType shaderType, out ShaderCreateInfo shaderCi)
        {
            shaderCi = new ShaderCreateInfo
            {
                Type = shaderType
            };
            string assetPath;
            switch (shaderType)
            {
                case ShaderType.Vertex:
                {
                    shaderCi.Name = $"[Vertex]{nameof(SpriteEffect)}";
                    assetPath = "Shaders/spritebatch_vs.hlsl";
                }
                    break;
                case ShaderType.Pixel:
                {
                    shaderCi.Name = $"[Pixel]{nameof(SpriteEffect)}";
                    assetPath = "Shaders/spritebatch_ps.hlsl";
                }
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

        public static SpriteEffect Build(IServiceProvider provider)
        {
            return ActivatorExtended.CreateInstance<SpriteEffect>(provider) ?? throw new NullReferenceException();
        }
    }

    public class TextureSpriteEffect(
        IAssetManager assetManager,
        IPipelineStateManager pipelineStateManager,
        GraphicsSettings settings,
        IShaderResourceBindingCache shaderResourceBindingCache,
        IBufferManager bufferManager,
        IShaderManager shaderManager
    ) : SpriteEffect(assetManager, pipelineStateManager, settings, shaderResourceBindingCache, bufferManager,
        shaderManager)
    {
        private ITexture pTexture = assetManager.GetAsset<TextureAsset>("Textures/no_texture_dummy.jpg").Texture;

        public ITexture Texture
        {
            get => pTexture;
            set
            {
                if (pTexture == value)
                    return;
                pTexture = value;
                mDirtySrb = true;
            }
        }

        protected override void OnGetShaderCreateInfo(ShaderType shaderType, out ShaderCreateInfo shaderCi)
        {
            base.OnGetShaderCreateInfo(shaderType, out shaderCi);
            shaderCi.Macros.Add("RENGINE_ENABLED_TEXTURE", "1");
        }

        protected override ResourceMapping OnGetResourceMapping()
        {
            var resMapping = base.OnGetResourceMapping();
            resMapping.Add(ShaderTypeFlags.Pixel, TextureNames.MainTexture, pTexture);
            return resMapping;
        }

        protected override void OnCreatePipelineDesc(out GraphicsPipelineDesc desc)
        {
            base.OnCreatePipelineDesc(out desc);
            desc.Samplers.Add(new ImmutableSamplerDesc
            {
                Name = TextureNames.MainTexture,
                Sampler = new SamplerStateDesc(TextureFilterMode.Default, TextureAddressMode.Clamp)
            });
        }

        public static TextureSpriteEffect Build(IServiceProvider provider)
        {
            return ActivatorExtended.CreateInstance<TextureSpriteEffect>(provider) ??
                   throw new NullReferenceException();
        }
    }

    public class InstancedSpriteEffect(
        IPipelineStateManager pipelineStateManager,
        GraphicsSettings settings,
        IAssetManager assetManager,
        IShaderResourceBindingCache shaderResourceBindingCache,
        IBufferManager bufferManager,
        IShaderManager shaderManager
    ) : DefaultSpriteEffect(pipelineStateManager, settings, shaderResourceBindingCache, bufferManager, shaderManager)
    {
        protected override void OnGetShaderCreateInfo(ShaderType shaderType, out ShaderCreateInfo shaderCi)
        {
            shaderCi = new ShaderCreateInfo
            {
                Type = shaderType
            };
            string assetPath;
            switch (shaderType)
            {
                case ShaderType.Vertex:
                {
                    shaderCi.Name = $"[Instanced][Vertex]{nameof(SpriteEffect)}";
                    assetPath = "Shaders/spritebatch_instanced_vs.hlsl";
                }
                    break;
                case ShaderType.Pixel:
                {
                    shaderCi.Name = $"[Pixel]{nameof(SpriteEffect)}";
                    assetPath = "Shaders/spritebatch_ps.hlsl";
                }
                    break;
                case ShaderType.Compute:
                case ShaderType.Geometry:
                case ShaderType.Hull:
                case ShaderType.Domain:
                default:
                    throw new NotImplementedException();
            }

            shaderCi.SourceCode = assetManager.GetAsset<ShaderAsset>(assetPath).ShaderCode;
            shaderCi.Macros.Add("RENGINE_INSTANCED", "1");
        }

        protected override void OnCreatePipelineDesc(out GraphicsPipelineDesc desc)
        {
            base.OnCreatePipelineDesc(out desc);

            for (var i = 0u; i < 4; ++i)
            {
                desc.InputLayouts.Add(new PipelineInputLayoutElementDesc()
                {
                    InputIndex = i,
                    Input = new InputLayoutElementDesc()
                    {
                        BufferIndex = 0,
                        ElementType = ElementType.Vector4,
                        InstanceStepRate = 1
                    }
                });
            }
        }

        public static InstancedSpriteEffect Build(IServiceProvider provider)
        {
            return ActivatorExtended.CreateInstance<InstancedSpriteEffect>(provider) ??
                   throw new NullReferenceException();
        }
    }

    public class TextureInstancedSpriteEffect(
        IPipelineStateManager pipelineStateManager,
        GraphicsSettings settings,
        IAssetManager assetManager,
        IShaderResourceBindingCache shaderResourceBindingCache,
        IBufferManager bufferManager,
        IShaderManager shaderManager
    ) : InstancedSpriteEffect(
        pipelineStateManager,
        settings,
        assetManager,
        shaderResourceBindingCache,
        bufferManager,
        shaderManager)
    {
        private TextureAsset pTexture = assetManager.GetAsset<TextureAsset>("Textures/no_texture_dummy.jpg");

        public TextureAsset Texture
        {
            get => pTexture;
            set
            {
                if (pTexture == value)
                    return;
                pTexture = value;
                mDirtySrb = true;
            }
        }

        protected override void OnGetShaderCreateInfo(ShaderType shaderType, out ShaderCreateInfo shaderCi)
        {
            base.OnGetShaderCreateInfo(shaderType, out shaderCi);
            shaderCi.Macros.Add("RENGINE_ENABLED_TEXTURE", "1");
        }

        protected override void OnCreatePipelineDesc(out GraphicsPipelineDesc desc)
        {
            base.OnCreatePipelineDesc(out desc);
            desc.Samplers.Add(new ImmutableSamplerDesc
            {
                Name = TextureNames.MainTexture,
                Sampler = new SamplerStateDesc(TextureFilterMode.Default, TextureAddressMode.Clamp)
            });
        }

        protected override ResourceMapping OnGetResourceMapping()
        {
            var resMapping = base.OnGetResourceMapping();
            resMapping.Add(ShaderTypeFlags.Pixel, TextureNames.MainTexture, pTexture.Texture);
            return resMapping;
        }

        public static TextureInstancedSpriteEffect Build(IServiceProvider provider)
        {
            return ActivatorExtended.CreateInstance<TextureInstancedSpriteEffect>(provider) ??
                   throw new NullReferenceException();
        }
    }
}