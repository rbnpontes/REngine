using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable All
namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct GraphicsPipelineDescDTO
	{
		public IntPtr name;

		public byte blendState_colorWriteEnabled;
		public byte blendState_blendMode;
		public byte blendState_alphaToCoverage;

		public byte		rasterizerState_fillMode;
		public byte		rasterizerState_cullMode;
		public float	rasterizerState_constantDepthBias;
		public float	rasterizerState_slopeScaledDepthBias;
		public byte		rasterizerState_scissorTestEnabled;
		public byte		rasterizerState_lineAntiAlias;

		public byte depthStencilState_enableDepth;
		public byte depthStencilState_depthWriteEnabled;
		public byte depthStencilState_stencilTestEnabled;
		public byte depthStencilState_depthCmpFunc;
		public byte depthStencilState_stencilCmpFunc;
		public byte depthStencilState_stencilOpOnPassed;
		public byte depthStencilState_stencilOpOnStencilFailed;
		public byte depthStencilState_stencilOpOnDepthFailed;
		public byte depthStencilState_stencilCmpMask;
		public byte depthStencilState_stencilWriteMask;

		public byte primitiveType;

		public IntPtr	inputLayouts;
		public byte		numInputLayouts;

		public ushort	output_depthStencilFormat;
		public IntPtr	output_rtFormats;
		public byte		output_numRtFormats;
		public byte		output_multiSample;

		public IntPtr	immutableSamplers;
		public byte		numImmutableSamplers;

		public IntPtr shader_vs;
		public IntPtr shader_ps;
		public IntPtr shader_ds;
		public IntPtr shader_hs;
		public IntPtr shader_gs;

		public IntPtr pscache;

		public static void Fill(in GraphicsPipelineDesc desc, out GraphicsPipelineDescDTO output)
		{
			output = new GraphicsPipelineDescDTO()
			{
				name = string.IsNullOrEmpty(desc.Name) ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(desc.Name),

				blendState_colorWriteEnabled = (byte)(desc.BlendState.ColorWriteEnabled ? 1 : 0),
				blendState_blendMode = (byte)desc.BlendState.BlendMode,
				blendState_alphaToCoverage = (byte)(desc.BlendState.AlphaToCoverageEnabled ? 1 : 0),

				rasterizerState_fillMode = (byte)desc.RasterizerState.FillMode,
				rasterizerState_cullMode = (byte)desc.RasterizerState.CullMode,
				rasterizerState_constantDepthBias = desc.RasterizerState.ConstantDepthBias,
				rasterizerState_slopeScaledDepthBias = desc.RasterizerState.SlopeScaledDepthBias,
				rasterizerState_scissorTestEnabled = (byte)(desc.RasterizerState.ScissorTestEnabled ? 1 : 0),
				rasterizerState_lineAntiAlias = (byte)(desc.RasterizerState.LineAntiAlias ? 1 : 0),

				depthStencilState_enableDepth = (byte)(desc.DepthStencilState.EnableDepth ? 1 : 0),
				depthStencilState_depthWriteEnabled = (byte)(desc.DepthStencilState.DepthWriteEnabled ? 1 : 0),
				depthStencilState_stencilTestEnabled = (byte)(desc.DepthStencilState.StencilTestEnabled ? 1 : 0),
				depthStencilState_depthCmpFunc = (byte)desc.DepthStencilState.DepthCompareFunction,
				depthStencilState_stencilCmpFunc = (byte)desc.DepthStencilState.StencilCompareFunction,
				depthStencilState_stencilOpOnPassed = (byte)desc.DepthStencilState.StencilOpOnPassed,
				depthStencilState_stencilOpOnStencilFailed = (byte)desc.DepthStencilState.StencilOpOnStencilFailed,
				depthStencilState_stencilOpOnDepthFailed = (byte)desc.DepthStencilState.StencilOpOnDepthFailed,
				depthStencilState_stencilCmpMask = desc.DepthStencilState.StencilCompareMask,
				depthStencilState_stencilWriteMask = desc.DepthStencilState.StencilWriteMask,

				primitiveType = (byte)desc.PrimitiveType,

				numInputLayouts = (byte)desc.InputLayouts.Count,

				output_depthStencilFormat = (ushort)desc.Output.DepthStencilFormat,
				output_numRtFormats = (byte)desc.Output.RenderTargetFormats.Count,
				output_multiSample = desc.Output.MultiSample,

				numImmutableSamplers = (byte)desc.Samplers.Count,

				shader_vs = desc.Shaders.VertexShader?.Handle ?? IntPtr.Zero,
				shader_ps = desc.Shaders.PixelShader?.Handle ?? IntPtr.Zero,
				shader_ds = desc.Shaders.DomainShader?.Handle ?? IntPtr.Zero,
				shader_hs = desc.Shaders.HullShader?.Handle ?? IntPtr.Zero,
				shader_gs = desc.Shaders.GeometryShader?.Handle ?? IntPtr.Zero,

				pscache =  desc.PSCache?.Handle ?? IntPtr.Zero,
			};
		}
	}
}
