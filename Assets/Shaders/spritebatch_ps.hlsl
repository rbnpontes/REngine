﻿#ifdef RENGINE_ENABLED_TEXTURE
Texture2D g_texture;
SamplerState g_texture_sampler;
#endif

struct PSInput {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float4 color : COLOR0;
};

float4 main(in PSInput ps_input) : SV_TARGET{
	float4 result = float4(1.0, 1.0, 1.0, 1.0);
	float2 uv = ps_input.uv;
#ifdef RENGINE_ENABLED_TEXTURE
	result = g_texture.Sample(g_texture_sampler, uv);
#endif
	result = result * ps_input.color;
	return result;
}