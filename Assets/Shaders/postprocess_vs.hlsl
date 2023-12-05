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

void main(uint id : SV_VertexID, out PSOutput output)
{
    output.uv = float2((id << 1) & 2, id & 2);
    output.pos = float4(output.uv * float2(2, -2) + float2(-1, 1), 1, 1);
    output.worldPos = mul(g_invScreenProj, output.pos);
    output.worldPos *= 1.0f / output.worldPos.w;
    output.screenSize = float2(g_screenWidth, g_screenHeight);
    output.time = float2(g_elapsedTime, g_deltaTime);
}