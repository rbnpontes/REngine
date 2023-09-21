#if RENGINE_ENABLED_TEXTURE
Texture2D g_texture;
SamplerState g_texture_sampler;
#endif

struct PSInput {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float4 color : COLOR0;
};

float4 main(in PSInput input) : SV_TARGET{
	float4 result = float4(1.0, 1.0, 1.0, 1.0);
#if RENGINE_ENABLED_TEXTURE
	result = g_texture.Sample(g_texture_sampler, input.uv);
#endif
	result = result * input.color;
	return result;
}