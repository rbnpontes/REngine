using REngine.Core.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public struct PipelineBlendState
	{
		public bool ColorWriteEnabled;
		public BlendMode BlendMode;
		public bool AlphaToCoverageEnabled;

		public PipelineBlendState()
		{
			ColorWriteEnabled = true;
			BlendMode = BlendMode.Replace;
			AlphaToCoverageEnabled = false;
		}
	}

	public struct PipelineRasterizerState
	{
		public FillMode FillMode;
		public CullMode CullMode;
		public float ConstantDepthBias;
		public float SlopeScaledDepthBias;
		public bool ScissorTestEnabled;
		public bool LineAntiAlias;
	}
	
	public struct PipelineDepthStencilState
	{
		public bool EnableDepth;
		public bool DepthWriteEnabled;
		public bool StencilTestEnabled;
		public CompareMode DepthCompareFunction;
		public CompareMode StencilCompareFunction;
		public StencilOp StencilOpOnPassed;
		public StencilOp StencilOpOnStencilFailed;
		public StencilOp StencilOpOnDepthFailed;
		public byte StencilCompareMask;
		public byte StencilWriteMask;

		public PipelineDepthStencilState()
		{
			this = default(PipelineDepthStencilState);
			EnableDepth = true;
		}
	}

	public struct PipelineStateOutputDesc
	{
		public TextureFormat DepthStencilFormat;
		public IList<TextureFormat> RenderTargetFormats;
		public byte MultiSample;

		public PipelineStateOutputDesc()
		{
			DepthStencilFormat = TextureFormat.Unknown;
			RenderTargetFormats = new List<TextureFormat>() { TextureFormat.Unknown };
			MultiSample = 1;
		}
	}

	public struct PipelineInputLayoutElementDesc
	{
		public uint InputIndex;
		public InputLayoutElementDesc Input;
	}

	public struct GraphicsPipelineShaders
	{
		public IShader? VertexShader;
		public IShader? PixelShader;
		public IShader? DomainShader;
		public IShader? HullShader;
		public IShader? GeometryShader;
	}

	public struct GraphicsPipelineDesc
	{
		public string Name;
		public PipelineBlendState BlendState;
		public PipelineRasterizerState RasterizerState;
		public PipelineDepthStencilState DepthStencilState;
		public PrimitiveType PrimitiveType;
		public IList<PipelineInputLayoutElementDesc> InputLayouts;
		public PipelineStateOutputDesc Output;
		public IList<ImmutableSamplerDesc> Samplers;

		public GraphicsPipelineShaders Shaders;

		public GraphicsPipelineDesc()
		{
			Name = string.Empty;
			BlendState = new PipelineBlendState();
			RasterizerState = new PipelineRasterizerState();
			DepthStencilState = new PipelineDepthStencilState();
			PrimitiveType = PrimitiveType.TriangleList;
			InputLayouts = new List<PipelineInputLayoutElementDesc>();
			Samplers = new List<ImmutableSamplerDesc>();
			Output = new PipelineStateOutputDesc();
			Shaders = new GraphicsPipelineShaders();
		}
	}

	public struct ComputePipelineDesc
	{
		public string Name;
		public IList<ImmutableSamplerDesc> Samplers;
		public IShader? ComputeShader;

		public ComputePipelineDesc()
		{
			Name = string.Empty;
			Samplers = new List<ImmutableSamplerDesc>();
			ComputeShader = null;
		}
	}
}
