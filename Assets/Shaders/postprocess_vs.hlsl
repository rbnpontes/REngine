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

void main(uint vs_id : SV_VertexID, out PSOutput ps_input)
{
    ps_input.uv = float2((vs_id << 1) & uint(2), vs_id & uint(2));
    ps_input.pos = float4(ps_input.uv * float2(2, -2) + float2(-1, 1), 1, 1);
    ps_input.worldPos = mul(g_invScreenProj, ps_input.pos);
    ps_input.worldPos *= 1.0f / ps_input.worldPos.w;
    ps_input.screenSize = float2(g_screenWidth, g_screenHeight);
    ps_input.time = float2(g_elapsedTime, g_deltaTime);
}