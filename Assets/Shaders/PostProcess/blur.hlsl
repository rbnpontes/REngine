// Fast Gaussian Blur:
// https://www.shadertoy.com/view/Xltfzj
// https://xorshaders.weebly.com/tutorials/blur-shaders-5-part-2

cbuffer MaterialConstants
{
    float g_directions;
    float g_quality;
    float g_size;
};

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

#define RENGINE_DOUBLE_PI 6.28318530718

float4 main(in PSInput vs_input) : SV_TARGET
{
    float2 radius = float2(g_size, g_size) / vs_input.screenSize;
    float2 uv = vs_input.uv;

    float initialQuality = 1.0 / g_quality;

    float incr = RENGINE_DOUBLE_PI / g_directions;
    float qualityIncr = 1.0 / g_quality;

    float4 color = g_texture.Sample(g_texture_sampler, uv);

    for (float d = 0.0; d < RENGINE_DOUBLE_PI; d += incr)
    {
        for (float i = initialQuality; i <= 1.0; i += qualityIncr)
        {
            color += g_texture.Sample(g_texture_sampler, uv + float2(cos(d), sin(d)) * radius * i);
        }
    }

    color /= g_quality * g_directions - 15.0;
    return color;
}