#ifdef RENGINE_INSTANCED
cbuffer Constants {
	float4x4 g_projection;
	float4 g_color;
};
#else
cbuffer Constants {
	float4x4 g_transform;
	float4x4 g_projection;
	float4 g_color;
};
#endif

#ifdef RENGINE_INSTANCED
struct PSInput {
	float2 pos : ATTRIB0;
	float2 uv : ATTRIB1;
	// Instance values
	float4 instRow0 : ATTRIB2;
	float4 instRow1 : ATTRIB3;
	float4 instRow2 : ATTRIB4;
	float4 instRow3 : ATTRIB5;
};
#else
struct PSInput {
	float2 pos : ATTRIB0;
	float2 uv : ATTRIB1;
};
#endif

struct PSOutput {
	float4 pos		: SV_POSITION;
	float2 uv		: TEXCOORD0;
	float4 color	: COLOR0;
};
void main(in PSInput input, out PSOutput output) 
{
#ifdef RENGINE_INSTANCED
	float4x4 transform = MatrixFromRows(input.instRow0, input.instRow1, input.instRow2, input.instRow3);
	float4 pos = mul(mul(g_projection, transform), float4(input.pos, 0.0, 1.0));
#else
	float4 pos = mul(mul(g_projection, g_transform), float4(input.pos, 0.0, 1.0));
#endif
	output.pos = pos;
	output.uv = input.uv;
	output.color = g_color;
}