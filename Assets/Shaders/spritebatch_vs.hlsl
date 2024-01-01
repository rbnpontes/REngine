cbuffer ObjectConstants {
	float4x4 g_transform;
	float4 g_color;
};
cbuffer FrameConstants
{
    float4x4 g_projection;
};

struct vs_input {
	uint vertex_id : SV_VertexID;
};
struct PSOutput {
	float4 pos		: SV_POSITION;
	float2 uv		: TEXCOORD0;
	float4 color	: COLOR0;
};
void main(in vs_input vs_input, out PSOutput ps_input) 
{
	float2 vertices[4];
	vertices[0] = float2(0, 1);
	vertices[1] = float2(0, 0);
	vertices[2] = float2(1, 1);
	vertices[3] = float2(1, 0);
	float2 uvs[4];
	uvs[0] = float2(0, 0);
	uvs[1] = float2(0, 1);
	uvs[2] = float2(1, 0);
	uvs[3] = float2(1, 1);

	float4 pos = mul(g_transform, float4(vertices[vs_input.vertex_id], 0.0, 1.0));
	ps_input.pos = pos;
	ps_input.uv = uvs[vs_input.vertex_id];
	ps_input.color = g_color;
}