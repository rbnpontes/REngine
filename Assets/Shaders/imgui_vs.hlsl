cbuffer FrameConstants
{
    // OpenGL don't like matrix as uniform constants
    float4x4 g_screenProj;
};

struct VSInput
{
    float2 pos      : ATTRIB0;
    float2 uv       : ATTRIB1;
    float4 color    : ATTRIB2;
};

struct PSOutput
{
    float4 pos      : SV_POSITION;
    float4 color    : COLOR;
    float2 uv       : TEXCOORD;
};

void main(in VSInput vs_input, out PSOutput ps_input)
{
    //ps_output.pos = g_screenProj * float4(vs_input.pos.xy, 0.0, 1.0);
    ps_input.pos = mul(g_screenProj, float4(vs_input.pos.xy, 0.0, 1.0));
    ps_input.color = vs_input.color;
    ps_input.uv = vs_input.uv;
}