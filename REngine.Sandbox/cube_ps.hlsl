Texture2D    g_MainTexture;
SamplerState g_MainTexture_sampler;


struct PSInput
{
	float4 pos		: SV_POSITION;
	float2 uv		: TEXCOORD;
};

struct PSOutput
{
	float4 color	 : SV_TARGET;
};

float4 BgraToRgba(float4 pixel) {
	return float4(pixel.b, pixel.g, pixel.r, pixel.a);
}

void main(in PSInput input, out PSOutput output)
{
	// .NET reads image as BGRA format, then we need to change to RGBA
	output.color = BgraToRgba(g_MainTexture.Sample(g_MainTexture_sampler, input.uv));
}