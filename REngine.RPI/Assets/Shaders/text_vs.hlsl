cbuffer Constants
{
	float4x4 g_projection;
	float4 g_color;
};

struct VSInput
{
	float2 pos : ATTRIB0;
	float2 uv : ATTRIB1;
};

struct PSOutput
{
	float4 pos : SV_POSITION;
	float4 color : COLOR;
	float2 uv : TEXCOORD;
};

void main(in VSInput input, out PSOutput output)
{
	output.pos = mul(g_projection, float4(input.pos.xy, 0.0, 1.0));
	output.color = g_color;
	output.uv = input.uv;
}