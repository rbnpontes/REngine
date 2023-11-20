using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.IO;
using REngine.RHI;
using REngine.RPI.Constants;

namespace REngine.RPI.Features
{
	public abstract class PostProcessFeature : BaseRenderFeature
	{
		private ITextureView? pReadTexture;
		private ITextureView? pWriteRenderTarget;
		private ITextureView? pDepthStencil;

		private IPipelineState? pPipeline;
		private IShaderResourceBinding? pBinding;

		public ITextureView? ReadTexture
		{
			get => pReadTexture;
			set
			{
				if(value == pReadTexture) 
					return;

				if(value != null)
					pBinding?.Set(ShaderTypeFlags.Pixel, TextureNames.MainTexture, value);
				pReadTexture = value;
			}
		}
		// ReSharper disable once InconsistentNaming
		public ITextureView? WriteRT
		{
			get => pWriteRenderTarget;
			set
			{
				IsDirty |= pWriteRenderTarget != value;
				pWriteRenderTarget = value;
			}
		}
		public ITextureView? DepthStencil
		{
			get => pDepthStencil;
			set
			{
				IsDirty |= pDepthStencil != value;
				pDepthStencil = value;
			}
		}
		public override bool IsDirty { get; protected set; } = true;

		public override IRenderFeature MarkAsDirty()
		{
			IsDirty = true;
			return this;
		}

		protected override void OnDispose()
		{
			pBinding?.Dispose();
		}

		protected override void OnSetup(in RenderFeatureSetupInfo setupInfo)
		{
			pPipeline = null;
			pBinding?.Dispose();

			pPipeline = OnBuildPipelineState(setupInfo);
			pBinding = pPipeline.CreateResourceBinding();

			pReadTexture ??= setupInfo.RenderTargetManager.GetDummyTexture().GetDefaultView(TextureViewType.ShaderResource);
			pWriteRenderTarget ??= setupInfo.Renderer.SwapChain?.ColorBuffer;
			pDepthStencil ??= setupInfo.Renderer.SwapChain?.DepthBuffer;

			OnSetBindings(pBinding, setupInfo.BufferManager);

			IsDirty = false;
		}

		protected override void OnExecute(ICommandBuffer command)
		{
			if (pPipeline is null || pBinding is null || pWriteRenderTarget is null || pReadTexture is null) 
				return;

			command
				.SetRT(pWriteRenderTarget, pDepthStencil)
				.SetPipeline(pPipeline)
				.CommitBindings(pBinding)
				.Draw(new DrawArgs()
				{
					NumInstances = 1,
					FirstInstanceLocation = 0,
					NumVertices = 3
				});
		}

		protected virtual IPipelineState OnBuildPipelineState(in RenderFeatureSetupInfo setupInfo)
		{
			var vsShader = LoadShader(setupInfo.ShaderManager, ShaderType.Vertex);
			var psShader = LoadShader(setupInfo.ShaderManager, ShaderType.Pixel);

			GraphicsPipelineDesc desc = new();
			if (pWriteRenderTarget is not null)
				desc.Output.RenderTargetFormats[0] = pWriteRenderTarget.Desc.Format;
			else
				desc.Output.RenderTargetFormats[0] = setupInfo.GraphicsSettings.DefaultColorFormat;

			desc.Output.DepthStencilFormat = pDepthStencil is not null ? pDepthStencil.Desc.Format 
				: setupInfo.GraphicsSettings.DefaultDepthFormat;

			desc.BlendState.BlendMode = BlendMode.Replace;
			desc.PrimitiveType = PrimitiveType.TriangleList;
			desc.RasterizerState.CullMode = CullMode.Back;
			desc.DepthStencilState.EnableDepth = false;

			desc.Shaders.VertexShader = vsShader;
			desc.Shaders.PixelShader = psShader;

			OnSetImmutableSamplers(desc.Samplers);

			return setupInfo.PipelineStateManager.GetOrCreate(desc);
		}

		private IShader LoadShader(IShaderManager shaderManager, ShaderType shaderType)
		{
			ShaderCreateInfo shaderCI = new()
			{
				Type = shaderType
			};
			ShaderStream shaderStream; 

			switch (shaderType)
			{
				case ShaderType.Vertex:
				{
					shaderCI.Name = "PostProcess Vertex Shader";
					shaderStream = new FileShaderStream(
						Path.Join(EngineSettings.AssetsShadersPath, "postprocess_vs.hlsl")
					);
				}
					break;
				case ShaderType.Pixel:
				{
					shaderCI.Name = "PostProcess Pixel Shader";
					shaderStream = OnGetShaderCode();
				}
					break;
				case ShaderType.Compute:
				case ShaderType.Geometry:
				case ShaderType.Hull:
				case ShaderType.Domain:
				default:
					throw new NotImplementedException(shaderType.ToString());
			}

			shaderCI.SourceCode = shaderStream.GetShaderCode();
			shaderStream.Dispose();

			return shaderManager.GetOrCreate(shaderCI);
		}

		protected virtual void OnSetImmutableSamplers(IList<ImmutableSamplerDesc> samplers)
		{
			samplers.Add(new ImmutableSamplerDesc()
			{
				Name = TextureNames.MainTexture,
				Sampler = new SamplerStateDesc(TextureFilterMode.Bilinear, TextureAddressMode.Clamp)
			});
		}

		protected virtual void OnSetBindings(IShaderResourceBinding binding, IBufferManager bufferManager)
		{
			binding.Set(ShaderTypeFlags.Vertex,ConstantBufferNames.Frame, bufferManager.GetBuffer(BufferGroupType.Frame));
			if(pReadTexture != null)
				binding.Set(ShaderTypeFlags.Pixel, TextureNames.MainTexture, pReadTexture);
		}
		protected abstract ShaderStream OnGetShaderCode();
	}
}
