﻿Texture2D g_texture;
SamplerState g_texture_sampler;

struct PSInput
{
    float4 pos      : SV_Position;
    float4 color    : COLOR;
    float2 uv       : TEXCOORD;
};

float4 main(in PSInput input) : SV_Target
{
    return input.color * g_texture.Sample(g_texture_sampler, input.uv);
}