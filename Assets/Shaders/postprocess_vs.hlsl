cbuffer FrameConstants
{
    float4x4 g_screenProj;
    float4x4 g_invScreenProj;
    uint g_screenWidth;
    uint g_screenHeight;
    float g_elapsedTime;
    float g_deltaTime;
};
struct PSOutput
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float2 screenSize : TEXCOORD1;
    float4 worldPos : TEXCOORD2;
    float2 time : TEXCOORD3;
};

void main(uint vs_id : SV_VertexID, out PSOutput ps_output)
{
    ps_output.uv = float2((vs_id << 1) & 2, vs_id & 2);
    ps_output.pos = float4(ps_output.uv * float2(2, -2) + float2(-1, 1), 1, 1);
    ps_output.worldPos = mul(g_invScreenProj, ps_output.pos);
    ps_output.worldPos *= 1.0f / ps_output.worldPos.w;
    ps_output.screenSize = float2(g_screenWidth, g_screenHeight);
    ps_output.time = float2(g_elapsedTime, g_deltaTime);
}