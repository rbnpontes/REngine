cbuffer Constants {
	float4 g_rotationAndScale;
	float4 g_position;
	float4 g_color;
};

struct PSInput {
	float4 pos		: SV_POSITION;
	float2 uv		: TEXCOORD0;
	float4 color	: COLOR0;
};
void main(in uint vertId : SV_VertexID, out PSInput input) 
{
	float4 pos_vertices[4];
	pos_vertices[0] = float4(+0.0, +0.0, 0.0, 0.0);
	pos_vertices[1] = float4(+0.0, -1.0, 0.0, 1.0);
	pos_vertices[2] = float4(+1.0, +0.0, 1.0, 0.0);
	pos_vertices[3] = float4(+1.0, -1.0, 1.0, 1.0);

	float2 position = pos_vertices[vertId].xy;
	// Diligent helper util. Convert Rotation and Scale to Transform Matrix2x2
	float2x2 transform = MatrixFromRows(g_rotationAndScale.xy, g_rotationAndScale.zw);
	position = mul(position, transform);
	// Offset Vertex Point by position to move batch
	position += g_position.xy;

	input.pos = float4(position, 0.0, 1.0);
	input.uv = pos_vertices[vertId].zw + g_position.zw;
	input.color = g_color;
}