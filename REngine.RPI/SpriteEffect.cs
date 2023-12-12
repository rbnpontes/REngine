using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RPI.Constants;

namespace REngine.RPI
{
	public abstract class SpriteEffect : IDisposable
	{
		public abstract bool IsDisposed { get; }
		public abstract string EffectName { get; }
		public abstract void Dispose();
		public abstract void OnSetMainTexture(ITexture? texture);
		public abstract IPipelineState OnBuildPipeline(IServiceProvider serviceProvider);
		public abstract IShaderResourceBinding OnGetSRB();
	}

	public class BasicSpriteEffect(string effectName, IAssetManager assetManager) : SpriteEffect
	{
		private bool pDisposed;
		private ITexture? pMainTexture;
		private IPipelineState? pPipelineState;
		private IShaderResourceBinding? pShaderResourceBinding;

		private ShaderStream? pVertexShaderStream;
		private ShaderStream? pPixelShaderStream;

		public override bool IsDisposed => pDisposed;

		public ShaderStream? VertexShader
		{
			get => pVertexShaderStream;
			set
			{
				AssertDispose();
				if (value == pVertexShaderStream) return;
				pPipelineState = null;
				pVertexShaderStream?.Dispose();
				pVertexShaderStream = value;
			}
		}

		public ShaderStream? PixelShader
		{
			get => pPixelShaderStream;
			set
			{
				AssertDispose();
				if (value == pPixelShaderStream) return;
				pPipelineState = null;
				pPixelShaderStream?.Dispose();
				pPixelShaderStream = value;
			}
		}

		public override string EffectName => effectName;

		private ShaderStream GetShaderStream(ShaderType type)
		{
			ShaderStream stream;
			switch (type)
			{
				case ShaderType.Vertex:
				{
					pVertexShaderStream ??=
						new StreamedShaderStream(assetManager.GetStream("Shaders/spritebatch_vs.hlsl"));
					stream = pVertexShaderStream;
				}
					break;
				case ShaderType.Pixel:
				{
					pPixelShaderStream ??=
						new StreamedShaderStream(assetManager.GetStream("Shaders/spritebatch_ps.hlsl"));
					stream = pPixelShaderStream;
				}
					break;
				case ShaderType.Compute:
				case ShaderType.Geometry:
				case ShaderType.Hull:
				case ShaderType.Domain:
				default:
					throw new NotSupportedException($"Not supported shader type {type}");
			}

			return stream;
		}

		public override void Dispose()
		{
			if(pDisposed) 
				return;

			pVertexShaderStream?.Dispose(); pPixelShaderStream?.Dispose();
			pVertexShaderStream = pPixelShaderStream = null;

			pShaderResourceBinding?.Dispose();

			pMainTexture = null;
			pPipelineState = null;
			pDisposed = true;
			
			GC.SuppressFinalize(this);
		}

		public override void OnSetMainTexture(ITexture? texture)
		{
			AssertDispose();
			if (pMainTexture == texture)
				return;

			if (texture != null)
			{
				if (pMainTexture is null)
				{
					pPipelineState = null;
					pShaderResourceBinding?.Dispose();
					pShaderResourceBinding = null;
				}
				else
					pShaderResourceBinding?.Set(ShaderTypeFlags.Pixel, TextureNames.MainTexture, texture.GetDefaultView(TextureViewType.ShaderResource));
			}
			else if(pMainTexture != null)
			{
				pPipelineState = null;
				pShaderResourceBinding?.Dispose();
				pShaderResourceBinding = null;
			}

			pMainTexture = texture;
		}

		public override IPipelineState OnBuildPipeline(IServiceProvider serviceProvider)
		{
			AssertDispose();
			if (pPipelineState != null)
				return pPipelineState;

			var psMgr = serviceProvider.Get<IPipelineStateManager>();
			GetPipelineStateDesc(serviceProvider, out var pipelineDesc);

			var result = psMgr.GetOrCreate(pipelineDesc);
			var srb = result.CreateResourceBinding();

			SetConstantBuffers(serviceProvider, srb);
			if(pMainTexture != null)
				srb.Set(ShaderTypeFlags.Pixel, TextureNames.MainTexture, pMainTexture.GetDefaultView(TextureViewType.ShaderResource));

			pPipelineState = result;
			pShaderResourceBinding = srb;

			return result;
		}

		public override IShaderResourceBinding OnGetSRB()
		{
			AssertDispose();
			if (pShaderResourceBinding is null)
				throw new NullReferenceException(
					"Shader Resource Binding is null. Did you initialized PipelineState ?");
			return pShaderResourceBinding;
		}

		protected virtual void GetPipelineStateDesc(IServiceProvider provider, out GraphicsPipelineDesc desc)
		{
			var shaderManager = provider.Get<IShaderManager>();
			var settings = provider.Get<GraphicsSettings>();

			var output = new GraphicsPipelineDesc
			{
				Name = $"Sprite Effect - {EffectName}"
			};
			output.Output.RenderTargetFormats[0] = settings.DefaultColorFormat;
			output.Output.DepthStencilFormat = settings.DefaultDepthFormat;
			output.BlendState.BlendMode = BlendMode.Alpha;
			output.PrimitiveType = PrimitiveType.TriangleList;
			output.RasterizerState.CullMode = CullMode.Both;
			output.DepthStencilState.EnableDepth = true;

			output.Shaders.VertexShader = GetShader(shaderManager, ShaderType.Vertex);
			output.Shaders.PixelShader = GetShader(shaderManager, ShaderType.Pixel);

			output.InputLayouts.Add(
				new PipelineInputLayoutElementDesc
				{
					InputIndex = 0,
					Input = new InputLayoutElementDesc
					{
						ElementType = ElementType.Vector2
					}
				}
			);
			output.InputLayouts.Add(
				new PipelineInputLayoutElementDesc
				{
					InputIndex = 1,
					Input = new InputLayoutElementDesc
					{
						ElementType = ElementType.Vector2
					}
				}
			);

			output.Samplers.Add(new ImmutableSamplerDesc
			{
				Name = TextureNames.MainTexture,
				Sampler = new SamplerStateDesc(TextureFilterMode.Trilinear, TextureAddressMode.Clamp)
			});

			desc = output;
		}

		protected virtual IShader GetShader(IShaderManager shaderManager, ShaderType type)
		{
			var shaderCI = new ShaderCreateInfo
			{
				Type = type,
				SourceCode = GetShaderStream(type).GetShaderCode()
			};

			if (pMainTexture is null) 
				return shaderManager.GetOrCreate(shaderCI);

			shaderCI.Name = shaderCI.Name + "(TEXTURED)";
			shaderCI.Macros.Add("RENGINE_ENABLED_TEXTURE", "1");

			return shaderManager.GetOrCreate(shaderCI);
		}

		protected virtual void SetConstantBuffers(IServiceProvider provider, IShaderResourceBinding srb)
		{
			var bufferManager = provider.Get<IBufferManager>();
			srb.Set(ShaderTypeFlags.Vertex | ShaderTypeFlags.Pixel, ConstantBufferNames.Frame, bufferManager.GetBuffer(BufferGroupType.Frame));
			srb.Set(ShaderTypeFlags.Vertex, ConstantBufferNames.Object, bufferManager.GetBuffer(BufferGroupType.Object));
		}

		private void AssertDispose()
		{
			if (pDisposed)
				throw new ObjectDisposedException(nameof(SpriteEffect));
		}

	}

	public sealed class DefaultSpriteEffect(IAssetManager assetManager)
		: BasicSpriteEffect(nameof(DefaultSpriteEffect), assetManager);
}
