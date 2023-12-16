Texture2D g_texture;
SamplerState g_texture_sampler;

struct PSInput
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float2 screenSize : TEXCOORD1;
    float4 worldPos : TEXCOORD2;
    float2 time : TEXCOORD3;
};

float4 main(in PSInput ps_input) : SV_TARGET
{
    float2 uv = ps_input.uv;
#ifdef GLSL
    uv.y = 1.0f - uv.y;
#endif
    float4 base = g_texture.Sample(g_texture_sampler, uv);
    float3 color = base.xyz;
    float3x3 sepiaTransform = float3x3(
        0.393, 0.768, 0.189,
        0.349, 0.686, 0.168,
        0.272, 0.534, 0.131
    );
    
    return float4(mul(sepiaTransform, color), base.a);
}