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
    float4 color = g_texture.Sample(g_texture_sampler, uv);
    float alpha = color.a;
    color = float4(1.0f, 1.0f, 1.0f, 1.0f) - color;
    color.a = alpha;
    return color;
}