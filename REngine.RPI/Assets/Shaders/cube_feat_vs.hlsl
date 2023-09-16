cbuffer Constants
{
	float4x4 g_worldViewProj;
};

struct VSInput
{
	float3 pos		: ATTRIB0;
	//float4 color	: ATTRIB1;
	//float2 uv		: ATTRIB1;
};

struct PSInput
{
	float4 pos		: SV_POSITION;
	//float4 color	: COLOR;
	//float2 uv		: TEXCOORD;
};

void main(in VSInput input, out PSInput output)
{
	output.pos = mul(float4(input.pos, 1.0), g_worldViewProj);
	//output.color = input.color;
	//output.uv = input.uv;
}