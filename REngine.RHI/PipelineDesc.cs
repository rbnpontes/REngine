using REngine.Core.Collections;
using REngine.Core.Mathematics;
using REngine.Core.Serialization;
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

		public ulong ToHash()
		{
			var hash = ColorWriteEnabled ? 1UL : 0UL;
			hash = Hash.Combine(hash, (ulong)BlendMode);
			return Hash.Combine(hash, AlphaToCoverageEnabled ? 1UL : 0UL);
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

		public unsafe ulong ToHash()
		{
			var hash = (ulong)FillMode;
			hash = Hash.Combine(hash, (ulong)CullMode);

			{
				var x = ConstantDepthBias;
				hash = Hash.Combine(hash, *((ulong*)(&x)));
				x = SlopeScaledDepthBias;
				hash = Hash.Combine(hash, *((ulong*)(&x)));
			}

			hash = Hash.Combine(hash, ScissorTestEnabled ? 1UL : 0UL);
			hash = Hash.Combine(hash, LineAntiAlias ? 1UL : 0UL);
			return hash;
		}
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

		public ulong ToHash()
		{
			var hash = EnableDepth ? 1UL : 0UL;
			hash = Hash.Combine(hash, DepthWriteEnabled ? 1UL : 0UL);
			hash = Hash.Combine(hash, StencilTestEnabled ? 1UL : 0UL);
			hash = Hash.Combine(hash, (ulong)DepthCompareFunction);
			hash = Hash.Combine(hash, (ulong)StencilCompareFunction);
			hash = Hash.Combine(hash, (ulong)StencilOpOnPassed);
			hash = Hash.Combine(hash, (ulong)StencilOpOnStencilFailed);
			hash = Hash.Combine(hash, (ulong)StencilOpOnDepthFailed);
			hash = Hash.Combine(hash, (ulong)StencilCompareMask);
			return Hash.Combine(hash, (ulong)StencilWriteMask);
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

		public ulong ToHash()
		{
			var hash = (ulong)DepthStencilFormat;
			foreach (var fmt in RenderTargetFormats)
				hash = Hash.Combine(hash, (ulong)fmt);
			return Hash.Combine(hash, (ulong)MultiSample);
		}
	}

	public struct PipelineInputLayoutElementDesc
	{
		public uint InputIndex;
		public InputLayoutElementDesc Input;

		public ulong ToHash()
		{
			return Hash.Combine(InputIndex, Input.ToHash());
		}
	}

	public struct GraphicsPipelineShaders
	{
		public IShader? VertexShader;
		public IShader? PixelShader;
		public IShader? DomainShader;
		public IShader? HullShader;
		public IShader? GeometryShader;

		public readonly ulong ToHash()
		{
			ulong hash = 0;
			if (VertexShader != null)
				hash = VertexShader.ToHash();
			if (PixelShader != null)
				hash = Hash.Combine(hash, PixelShader.ToHash());
			if (DomainShader != null)
				hash = Hash.Combine(hash, DomainShader.ToHash());
			if (HullShader != null)
				hash = Hash.Combine(hash, HullShader.ToHash());
			if (GeometryShader != null)
				hash = Hash.Combine(hash, GeometryShader.ToHash());
			return hash;
		}
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

		[SerializationIgnore]
		public GraphicsPipelineShaders Shaders;

		[SerializationIgnore]
		public IPipelineStateCache? PSCache;
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
			PSCache = null;
		}

		public readonly ulong ToHash()
		{
			var hash = Hash.Digest(Name);
			hash = Hash.Combine(hash, BlendState.ToHash());
			hash = Hash.Combine(hash, RasterizerState.ToHash());
			hash = Hash.Combine(hash, DepthStencilState.ToHash());
			hash = Hash.Combine(hash, (ulong)PrimitiveType);
			hash = InputLayouts.Aggregate(hash, (current, input) => Hash.Combine(current, input.ToHash()));
			hash = Hash.Combine(hash, Output.ToHash());
			hash = Samplers.Aggregate(hash, (current, sample) => Hash.Combine(current, sample.ToHash()));
			return Hash.Combine(hash, Shaders.ToHash());
		}
	}

	public struct ComputePipelineDesc
	{
		public string Name;
		public IList<ImmutableSamplerDesc> Samplers;
		[SerializationIgnore]
		public IShader? ComputeShader;

		public ComputePipelineDesc()
		{
			Name = string.Empty;
			Samplers = new List<ImmutableSamplerDesc>();
			ComputeShader = null;
		}

		public readonly ulong ToHash()
		{
			var hash = Hash.Digest(Name);
			hash = Samplers.Aggregate(hash, (current, immutableSampler) => Hash.Combine(current, immutableSampler.ToHash()));
			if(ComputeShader != null)
				hash = ComputeShader.ToHash();
			return hash;
		}
	}
}
