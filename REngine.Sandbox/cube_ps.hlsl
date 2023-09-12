Texture2D    g_MainTexture;
SamplerState g_MainTexture_sampler; // By convention, texture samplers must use the '_sampler' suffix


struct PSInput
{
	float4 pos		: SV_POSITION;
	float2 uv		: TEXCOORD;
};

struct PSOutput
{
	float4 color	 : SV_TARGET;
};

void main(in PSInput input, out PSOutput output)
{
	output.color = g_MainTexture.Sample(g_MainTexture_sampler, input.uv);
}