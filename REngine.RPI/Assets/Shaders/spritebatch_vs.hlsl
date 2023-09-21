cbuffer Constants {
	float4x4 g_worldViewProj;
	float4 g_color;
};

struct PSInput {
	float2 pos : ATTRIB0;
	float2 uv : ATTRIB1;
};
struct PSOutput {
	float4 pos		: SV_POSITION;
	float2 uv		: TEXCOORD0;
	float4 color	: COLOR0;
};
void main(in PSInput input, out PSOutput output) 
{
	output.pos = mul(g_worldViewProj, float4(input.pos, 0.0, 1.0));
	output.uv = input.uv;
	output.color = g_color;
}