cbuffer FrameConstants
{
    // OpenGL don't like matrix as uniform constants
    float4 g_screenProj_row0;
    float4 g_screenProj_row1;
    float4 g_screenProj_row2;
    float4 g_screenProj_row3;
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

void main(in VSInput input, out PSOutput output)
{
    float4x4 screenProjection = float4x4(
        g_screenProj_row0.x, g_screenProj_row0.y, g_screenProj_row0.z, g_screenProj_row0.w,
        g_screenProj_row1.x, g_screenProj_row1.y, g_screenProj_row1.z, g_screenProj_row1.w,
        g_screenProj_row2.x, g_screenProj_row2.y, g_screenProj_row2.z, g_screenProj_row2.w,
        g_screenProj_row3.x, g_screenProj_row3.y, g_screenProj_row3.z, g_screenProj_row3.w
    );
    
    output.pos = mul(screenProjection, float4(input.pos.xy, 0.0, 1.0));
    output.color = input.color;
    output.uv = input.uv;
}