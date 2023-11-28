Texture2D g_texture;
SamplerState g_texture_sampler;

#define SMOOTHING_SCALE 0.2

struct PSInput
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR0;
};

float4 main(in PSInput input) : SV_Target
{
    float d = g_texture.Sample(g_texture_sampler, input.uv).a;
    float a = smoothstep(0.5 - SMOOTHING_SCALE, 0.5 + SMOOTHING_SCALE, d);
    return float4(a, a, a, 1.0f);
	//const float smoothing = 0.5f;
	//const float d = g_texture.Sample(g_texture_sampler, input.uv).a;
 //   float alpha = smoothstep(SMOOTHING_OFFSET - smoothing, SMOOTHING_OFFSET + smoothing, d);
    
 //   return input.color * float4(1.0, 1.0, 1.0, alpha);
}