using Diligent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver.Adapters
{
	internal class PipelineStateAdapter
	{
		private static readonly Diligent.PrimitiveTopology[] pPrimitiveTopologies = new Diligent.PrimitiveTopology[]
		{
			Diligent.PrimitiveTopology.TriangleList,
			Diligent.PrimitiveTopology.LineList,
			Diligent.PrimitiveTopology.PointList,
			Diligent.PrimitiveTopology.TriangleStrip,
			Diligent.PrimitiveTopology.LineStrip
		};
		private static readonly Diligent.ComparisonFunction[] pComparisonFunctions = new Diligent.ComparisonFunction[]
		{
			Diligent.ComparisonFunction.Always,
			Diligent.ComparisonFunction.Equal,
			Diligent.ComparisonFunction.NotEqual,
			Diligent.ComparisonFunction.Less,
			Diligent.ComparisonFunction.LessEqual,
			Diligent.ComparisonFunction.Greater,
			Diligent.ComparisonFunction.GreaterEqual
		};
		private static readonly bool[] pIsBlendEnabled = new bool[]
		{
			false,
			true,
			true,
			true,
			true,
			true,
			true,
			true,
			true,
			true,
		};
		private static readonly Diligent.BlendFactor[] pSourceBlends = new Diligent.BlendFactor[]
		{
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.DestColor,
			Diligent.BlendFactor.SrcAlpha,
			Diligent.BlendFactor.SrcAlpha,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.InvDestAlpha,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.SrcAlpha,
			Diligent.BlendFactor.SrcAlpha,
		};
		private static readonly Diligent.BlendFactor[] pDestBlends = new Diligent.BlendFactor[]
		{
			Diligent.BlendFactor.Zero,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.Zero,
			Diligent.BlendFactor.InvSrcAlpha,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.InvSrcAlpha,
			Diligent.BlendFactor.DestAlpha,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.InvSrcAlpha,
		};
		private static readonly Diligent.BlendFactor[] pSourceAlphaBlends = new Diligent.BlendFactor[]
		{
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.DestColor,
			Diligent.BlendFactor.SrcAlpha,
			Diligent.BlendFactor.SrcAlpha,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.InvDestAlpha,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.SrcAlpha,
			Diligent.BlendFactor.Zero,
		};
		private static readonly Diligent.BlendFactor[] pDestAlphaBlends = new Diligent.BlendFactor[]
		{
			Diligent.BlendFactor.Zero,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.Zero,
			Diligent.BlendFactor.InvSrcAlpha,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.InvSrcAlpha,
			Diligent.BlendFactor.DestAlpha,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.One,
			Diligent.BlendFactor.InvSrcAlpha,
		};
		private static readonly Diligent.BlendOperation[] pBlendOperations = new Diligent.BlendOperation[]
		{
			Diligent.BlendOperation.Add,
			Diligent.BlendOperation.Add,
			Diligent.BlendOperation.Add,
			Diligent.BlendOperation.Add,
			Diligent.BlendOperation.Add,
			Diligent.BlendOperation.Add,
			Diligent.BlendOperation.Add,
			Diligent.BlendOperation.RevSubtract,
			Diligent.BlendOperation.RevSubtract,
			Diligent.BlendOperation.Add,
		};
		private static readonly Diligent.StencilOp[] pStencilOperations = new Diligent.StencilOp[]
		{
			Diligent.StencilOp.Keep,
			Diligent.StencilOp.Zero,
			Diligent.StencilOp.Replace,
			Diligent.StencilOp.IncrWrap,
			Diligent.StencilOp.DecrWrap,
		};
		private static readonly Diligent.CullMode[] pCullMode = new Diligent.CullMode[]
		{
			Diligent.CullMode.None,
			Diligent.CullMode.Back,
			Diligent.CullMode.Front,
		};
		private static readonly Diligent.FillMode[] pFillMode = new Diligent.FillMode[]
		{
			Diligent.FillMode.Solid,
			Diligent.FillMode.Wireframe,
		};

		private static readonly uint[] pNumComponents = new uint[]
		{
			1,
			1,
			2,
			3,
			4,
			4,
			4
		};
		private static readonly Diligent.ValueType[] pValueTypes = new Diligent.ValueType[]
		{
			Diligent.ValueType.Int32,
			Diligent.ValueType.Float32,
			Diligent.ValueType.Float32,
			Diligent.ValueType.Float32,
			Diligent.ValueType.Float32,
			Diligent.ValueType.UInt8,
			Diligent.ValueType.UInt8,
		};
		private static readonly bool[] pIsNormalized = new bool[]
		{
			false,
			false,
			false,
			false,
			false,
			false,
			true
		};

		private static readonly Diligent.FilterType[,] pMinMagFilters = new Diligent.FilterType[,]
		{
			{ Diligent.FilterType.Point, Diligent.FilterType.ComparisonPoint },
			{ Diligent.FilterType.Linear, Diligent.FilterType.ComparisonLinear },
			{ Diligent.FilterType.Linear, Diligent.FilterType.ComparisonLinear },
			{ Diligent.FilterType.Anisotropic, Diligent.FilterType.ComparisonAnisotropic },
			{ Diligent.FilterType.Point, Diligent.FilterType.ComparisonPoint },
		};
		private static readonly Diligent.FilterType[,] pMipFilters = new Diligent.FilterType[,]
		{
			{ Diligent.FilterType.Point, Diligent.FilterType.ComparisonPoint },
			{ Diligent.FilterType.Point, Diligent.FilterType.ComparisonPoint },
			{ Diligent.FilterType.Linear, Diligent.FilterType.ComparisonLinear },
			{ Diligent.FilterType.Anisotropic, Diligent.FilterType.ComparisonAnisotropic },
			{ Diligent.FilterType.Linear, Diligent.FilterType.Linear },
		};
		private static readonly Diligent.TextureAddressMode[] pAddressModes = new Diligent.TextureAddressMode[]
		{
			Diligent.TextureAddressMode.Wrap,
			Diligent.TextureAddressMode.Mirror,
			Diligent.TextureAddressMode.Clamp
		};

		private bool pIsOpenGL = false;
		private GraphicsSettings pSettings;

		public PipelineStateAdapter(GraphicsSettings settings, bool isOpenGL)
		{
			pIsOpenGL = isOpenGL;
			pSettings = settings;
		}

		public void Fill(in GraphicsPipelineDesc desc, out Diligent.GraphicsPipelineStateCreateInfo output)
		{
			output = new GraphicsPipelineStateCreateInfo();
			output.PSODesc.Name = desc.Name;

			if (desc.Shaders.VertexShader == null || desc.Shaders.PixelShader == null)
				throw new NullReferenceException("On graphics pipeline, Vertex Shader and Pixel Shader is required.");

			output.Vs = (desc.Shaders.VertexShader as INativeObject)?.Handle as Diligent.IShader;
			output.Ps = (desc.Shaders.PixelShader as INativeObject)?.Handle as Diligent.IShader;
			output.Ds = (desc.Shaders.DomainShader as INativeObject)?.Handle as Diligent.IShader;
			output.Hs = (desc.Shaders.HullShader as INativeObject)?.Handle as Diligent.IShader;
			output.Gs = (desc.Shaders.GeometryShader as INativeObject)?.Handle as Diligent.IShader;

			List<Diligent.LayoutElement> layoutElements = new List<LayoutElement>();
			List<Diligent.ImmutableSamplerDesc> immutableSamplers = new List<Diligent.ImmutableSamplerDesc>();

			Fill(desc.InputLayouts, layoutElements);
			Fill(desc.Samplers, immutableSamplers);

			output.GraphicsPipeline.InputLayout.LayoutElements = layoutElements.ToArray();
			output.PSODesc.ResourceLayout.ImmutableSamplers = immutableSamplers.ToArray();

			output.GraphicsPipeline.PrimitiveTopology = pPrimitiveTopologies[(int)desc.PrimitiveType];

			output.GraphicsPipeline.RTVFormats = desc
				.Output
				.RenderTargetFormats
				.Select(x => (Diligent.TextureFormat)x)
				.ToArray();
			output.GraphicsPipeline.NumRenderTargets = (byte)desc.Output.RenderTargetFormats.Count;
			output.GraphicsPipeline.DSVFormat = (Diligent.TextureFormat)desc.Output.DepthStencilFormat;
			output.GraphicsPipeline.SmplDesc.Count = desc.Output.MultiSample;

			output.GraphicsPipeline.BlendDesc.AlphaToCoverageEnable = desc.BlendState.AlphaToCoverageEnabled;
			output.GraphicsPipeline.BlendDesc.IndependentBlendEnable = false;

			if(desc.Output.RenderTargetFormats.Count > 0)
			{
				var blendModeIdx = (int)desc.BlendState.BlendMode;
				output.GraphicsPipeline.BlendDesc.RenderTargets = new RenderTargetBlendDesc[]
				{
					new RenderTargetBlendDesc
					{
						BlendEnable = pIsBlendEnabled[blendModeIdx],
						SrcBlend = pSourceBlends[blendModeIdx],
						DestBlend = pDestBlends[blendModeIdx],
						BlendOp = pBlendOperations[blendModeIdx],
						SrcBlendAlpha = pSourceAlphaBlends[blendModeIdx],
						DestBlendAlpha = pDestAlphaBlends[blendModeIdx],
						BlendOpAlpha = pBlendOperations[blendModeIdx],
						RenderTargetWriteMask = desc.BlendState.ColorWriteEnabled 
							? ColorMask.All
							: ColorMask.None
					}
				};
			}

			output.GraphicsPipeline.DepthStencilDesc = new DepthStencilStateDesc
			{
				DepthEnable = desc.DepthStencilState.EnableDepth,
				DepthWriteEnable = desc.DepthStencilState.DepthWriteEnabled,
				DepthFunc = pComparisonFunctions[(int)desc.DepthStencilState.DepthCompareFunction],
				StencilEnable = desc.DepthStencilState.StencilTestEnabled,
				StencilReadMask = desc.DepthStencilState.StencilCompareMask,
				StencilWriteMask = desc.DepthStencilState.StencilWriteMask,
				FrontFace = new StencilOpDesc
				{
					StencilFailOp = pStencilOperations[(int)desc.DepthStencilState.StencilOpOnStencilFailed],
					StencilDepthFailOp = pStencilOperations[(int)desc.DepthStencilState.StencilOpOnDepthFailed],
					StencilPassOp = pStencilOperations[(int)desc.DepthStencilState.StencilOpOnPassed],
					StencilFunc = pComparisonFunctions[(int)desc.DepthStencilState.StencilCompareFunction],
				},
				BackFace = new StencilOpDesc
				{
					StencilFailOp = pStencilOperations[(int)desc.DepthStencilState.StencilOpOnStencilFailed],
					StencilDepthFailOp = pStencilOperations[(int)desc.DepthStencilState.StencilOpOnDepthFailed],
					StencilPassOp = pStencilOperations [(int)desc.DepthStencilState.StencilOpOnPassed],
					StencilFunc = pComparisonFunctions[(int)desc.DepthStencilState.StencilCompareFunction]
				}
			};

			uint depthBits = 24;
			if (desc.Output.DepthStencilFormat == TextureFormat.D16UNorm)
				depthBits = 16;

			int scaledDepthBias = pIsOpenGL ? 0 : (int)(desc.RasterizerState.ConstantDepthBias * (1 << (int)depthBits));

			output.GraphicsPipeline.RasterizerDesc.FillMode = pFillMode[(int)desc.RasterizerState.FillMode];
			output.GraphicsPipeline.RasterizerDesc.CullMode = pCullMode[(int)desc.RasterizerState.CullMode];
			output.GraphicsPipeline.RasterizerDesc.FrontCounterClockwise = false;
			output.GraphicsPipeline.RasterizerDesc.DepthBias = scaledDepthBias;
			output.GraphicsPipeline.RasterizerDesc.SlopeScaledDepthBias = desc.RasterizerState.SlopeScaledDepthBias;
			output.GraphicsPipeline.RasterizerDesc.DepthClipEnable = true;
			output.GraphicsPipeline.RasterizerDesc.ScissorEnable = desc.RasterizerState.ScissorTestEnabled;
			output.GraphicsPipeline.RasterizerDesc.AntialiasedLineEnable = !pIsOpenGL && desc.RasterizerState.LineAntiAlias;

			output.PSODesc.ResourceLayout.DefaultVariableType = ShaderResourceVariableType.Dynamic;
		}
		public void Fill(IList<PipelineInputLayoutElementDesc> inputLayouts, IList<Diligent.LayoutElement> outputElements)
		{
			outputElements.Clear();

			foreach (var pipelineInput in inputLayouts)
			{
				outputElements.Add(new LayoutElement
				{
					InputIndex = pipelineInput.InputIndex,
					RelativeOffset = pipelineInput.Input.ElementOffset,
					NumComponents = pNumComponents[(int)pipelineInput.Input.ElementType],
					ValueType = pValueTypes[(int)pipelineInput.Input.ElementType],
					IsNormalized = pIsNormalized[(int)pipelineInput.Input.ElementType],
					BufferSlot = pipelineInput.Input.BufferIndex,
					Stride = pipelineInput.Input.BufferStride,
					Frequency = pipelineInput.Input.InstanceStepRate != 0 
					? Diligent.InputElementFrequency.PerInstance 
					: Diligent.InputElementFrequency.PerVertex,
					InstanceDataStepRate = pipelineInput.Input.InstanceStepRate
				});
			}
		}
		private void Fill(IList<ImmutableSamplerDesc> samplers, IList<Diligent.ImmutableSamplerDesc> output, Diligent.ShaderType shaderStages = Diligent.ShaderType.All)
		{
			output.Clear();

			foreach (var sampler in samplers)
			{
				var filterModeIdx = (int)(sampler.Sampler.FilterMode == TextureFilterMode.Default 
					? pSettings.DefaultTextureFilterMode 
					: sampler.Sampler.FilterMode);
				var shadowCmp = sampler.Sampler.ShadowCompare ? 1 : 0;
				output.Add(new Diligent.ImmutableSamplerDesc
				{
					ShaderStages = shaderStages,
					SamplerOrTextureName = sampler.Name,
					Desc = new SamplerDesc
					{
						MinFilter = pMinMagFilters[filterModeIdx, shadowCmp],
						MagFilter = pMinMagFilters[filterModeIdx, shadowCmp],
						MipFilter = pMipFilters[filterModeIdx, shadowCmp],
						AddressU = pAddressModes[(int)sampler.Sampler.AddressModes.U],
						AddressV = pAddressModes[(int)sampler.Sampler.AddressModes.V],
						AddressW = pAddressModes[(int)sampler.Sampler.AddressModes.W],
						MaxAnisotropy = sampler.Sampler.Anisotropy == 0 ? pSettings.DefaultTextureAnisotropy : sampler.Sampler.Anisotropy,
						ComparisonFunc = ComparisonFunction.Equal,
						MinLOD = uint.MinValue,
						MaxLOD = uint.MaxValue,
					}
				});
			}
		}
	}
}
