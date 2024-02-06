using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct GraphicsPipelineDescDto
{
    [FieldOffset(0)]
    public IntPtr Name;

    [FieldOffset(4)]
    public byte BlendState_ColorWriteEnabled;
    [FieldOffset(8)]
    public byte BlendState_BlendMode;
    [FieldOffset(12)]
    public byte BlendState_AlphaToCoverage;

    [FieldOffset(16)]
    public byte RasterizerState_FillMode;
    [FieldOffset(20)]
    public byte RasterizerState_CullMode;
    [FieldOffset(24)]
    public float RasterizerState_ConstantDepthBias;
    [FieldOffset(28)]
    public float RasterizerState_SlopeScaledDepthBias;
    [FieldOffset(32)]
    public byte RasterizerState_ScissorTestEnabled;
    [FieldOffset(36)]
    public byte RasterizerState_LineAntiAlias;

    [FieldOffset(40)]
    public byte DepthStencilState_EnableDepth;
    [FieldOffset(44)]
    public byte DepthStencilState_DepthWriteEnabled;
    [FieldOffset(48)]
    public byte DepthStencilState_StencilTestEnabled;
    [FieldOffset(52)]
    public byte DepthStencilState_DepthCmpFunc;
    [FieldOffset(56)]
    public byte DepthStencilState_StencilCmpFunc;
    [FieldOffset(60)]
    public byte DepthStencilState_StencilOpOnPassed;
    [FieldOffset(64)]
    public byte DepthStencilState_StencilOpOnStencilFailed;
    [FieldOffset(68)]
    public byte DepthStencilState_StencilOpOnDepthFailed;
    [FieldOffset(72)]
    public byte DepthStencilState_StencilCmpMask;
    [FieldOffset(76)]
    public byte DepthStencilState_StencilWriteMask;

    [FieldOffset(80)]
    public byte PrimitiveType;

    [FieldOffset(84)]
    public IntPtr InputLayouts;
    [FieldOffset(88)]
    public byte NumInputLayouts;

    [FieldOffset(92)]
    public ushort Output_DepthStencilFormat;
    [FieldOffset(96)]
    public IntPtr Output_RtFormats;
    [FieldOffset(100)]
    public byte Output_NumRtFormats;
    [FieldOffset(104)]
    public byte Output_MultiSample;

    [FieldOffset(108)]
    public IntPtr ImmutableSamplers;
    [FieldOffset(112)]
    public byte NumImmutableSamplers;

    [FieldOffset(116)]
    public IntPtr Shader_VS;
    [FieldOffset(120)]
    public IntPtr Shader_PS;
    [FieldOffset(124)]
    public IntPtr Shader_DS;
    [FieldOffset(128)]
    public IntPtr Shader_HS;
    [FieldOffset(132)]
    public IntPtr Shader_GS;

    [FieldOffset(136)]
    public IntPtr PSCache;

    public GraphicsPipelineDescDto()
    {
        Name = InputLayouts = Output_RtFormats = ImmutableSamplers
            = Shader_VS = Shader_PS = Shader_DS = Shader_HS = Shader_GS = IntPtr.Zero;
        PSCache = IntPtr.Zero;
        NumInputLayouts = Output_NumRtFormats = NumImmutableSamplers = 0;
    }

    public GraphicsPipelineDescDto(GraphicsPipelineDesc desc)
    {
        this = default;
        BlendState_ColorWriteEnabled = (byte)(desc.BlendState.ColorWriteEnabled ? 0x1 : 0x0);
        BlendState_BlendMode = (byte)desc.BlendState.BlendMode;
        BlendState_AlphaToCoverage = (byte)(desc.BlendState.AlphaToCoverageEnabled ? 0x1 : 0x0);

        RasterizerState_FillMode = (byte)desc.RasterizerState.FillMode;
        RasterizerState_CullMode = (byte)desc.RasterizerState.CullMode;
        RasterizerState_ConstantDepthBias = desc.RasterizerState.ConstantDepthBias;
        RasterizerState_SlopeScaledDepthBias = desc.RasterizerState.SlopeScaledDepthBias;
        RasterizerState_ScissorTestEnabled = (byte)(desc.RasterizerState.ScissorTestEnabled ? 0x1 : 0x0);
        RasterizerState_LineAntiAlias = (byte)(desc.RasterizerState.LineAntiAlias ? 0x1 : 0x0);

        DepthStencilState_EnableDepth = (byte)(desc.DepthStencilState.EnableDepth ? 0x1 : 0x0);
        DepthStencilState_DepthWriteEnabled = (byte)(desc.DepthStencilState.DepthWriteEnabled ? 0x1 : 0x0);
        DepthStencilState_StencilTestEnabled = (byte)(desc.DepthStencilState.StencilTestEnabled ? 0x1 : 0x0);
        DepthStencilState_DepthCmpFunc = (byte)desc.DepthStencilState.DepthCompareFunction;
        DepthStencilState_StencilCmpFunc = (byte)desc.DepthStencilState.StencilCompareFunction;
        DepthStencilState_StencilOpOnPassed = (byte)desc.DepthStencilState.StencilOpOnPassed;
        DepthStencilState_StencilOpOnStencilFailed = (byte)desc.DepthStencilState.StencilOpOnStencilFailed;
        DepthStencilState_StencilOpOnDepthFailed = (byte)desc.DepthStencilState.StencilOpOnDepthFailed;
        DepthStencilState_StencilCmpMask = desc.DepthStencilState.StencilCompareMask;
        DepthStencilState_StencilWriteMask = desc.DepthStencilState.StencilWriteMask;

        PrimitiveType = (byte)desc.PrimitiveType;
        
        NumInputLayouts = (byte)desc.InputLayouts.Count;

        Output_DepthStencilFormat = (ushort)desc.Output.DepthStencilFormat;
        Output_NumRtFormats = (byte)desc.Output.RenderTargetFormats.Count;
        Output_MultiSample = desc.Output.MultiSample;

        NumImmutableSamplers = (byte)desc.Samplers.Count;

        Shader_VS = desc.Shaders.VertexShader?.Handle ?? IntPtr.Zero;
        Shader_PS = desc.Shaders.PixelShader?.Handle ?? IntPtr.Zero;
        Shader_DS = desc.Shaders.DomainShader?.Handle ?? IntPtr.Zero;
        Shader_HS = desc.Shaders.HullShader?.Handle ?? IntPtr.Zero;
        Shader_GS = desc.Shaders.GeometryShader?.Handle ?? IntPtr.Zero;
    }

    public ref GraphicsPipelineDescDto GetPinnableReference()
    {
        return ref this;
    }
}