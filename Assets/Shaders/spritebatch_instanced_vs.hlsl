cbuffer ObjectConstants {
	float4 g_color;
};

cbuffer FrameConstants
{
    float4x4 g_projection;
};

// struct vs_data {
// 	uint vertex_id : SV_VertexID;
// 	// Instance values
// 	float3 row0 : ATTRIB0;
// 	float3 row1 : ATTRIB1;
// 	float3 row2 : ATTRIB2;
// };
struct vs_data
{
	uint vertex_id : SV_VertexID;
	float4 row0 : ATTRIB0;
	float4 row1 : ATTRIB1;
	float4 row2 : ATTRIB2;
	float4 row3 : ATTRIB3;
};

struct descompacted_data
{
	float2 position;
	float2 scale;
	float2 anchor;
	float2 cos_and_sin;
};


#define IDENTITY_MATRIX float4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)

float4x4 create_translate(float2 position) {
	float4x4 result = IDENTITY_MATRIX;
#ifdef GLSL
	result[0][3] = position.x;
	result[1][3] = position.y;
#else
	result[3][0] = position.x;
	result[3][1] = position.y;
#endif
	return result;
}
float4x4 create_scale(float2 scale) {
	float4x4 result = IDENTITY_MATRIX;
	result[0][0] = scale.x;
	result[1][1] = scale.y;
	return result;
}
float4x4 create_rotation(float2 cos_and_sin) {
	float4x4 result = IDENTITY_MATRIX;
	float c = cos_and_sin.x;
	float s = cos_and_sin.y;
	
#ifdef GLSL
	result[0][0] = c;
	result[1][0] = s;
	result[0][1] = -s;
	result[1][1] = c;
#else
	result[0][0] = c;
	result[0][1] = s;
	result[1][0] = -s;
	result[1][1] = c;
#endif
	
	return result;
}

void descompact_vs_data(in vs_data vs_data, out descompacted_data data_output)
{
	data_output.position = float2(vs_data.row0.x, vs_data.row0.y);
	data_output.cos_and_sin = float2(vs_data.row1.x, vs_data.row1.y);
	data_output.scale = float2(vs_data.row2.x, vs_data.row2.y);
	data_output.anchor = float2(vs_data.row1.z, vs_data.row2.z);
}

struct PSOutput {
	float4 pos		: SV_POSITION;
	float2 uv		: TEXCOORD0;
	float4 color	: COLOR0;
};
void main(in vs_data vs_input, out PSOutput ps_input) 
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
	
	// float4x4 transform = MatrixFromRows(
	// 	float4(vs_input.row0.x, vs_input.row0.y, 0, vs_input.row0.z),
	// 	float4(vs_input.row1.x, vs_input.row1.y, 0, vs_input.row1.z),
	// 	float4(vs_input.row2.x, vs_input.row2.y, vs_input.row2.z, 0),
	// 	float4(0, 0, 0, 1)
	// );

	// descompacted_data data;
	// descompact_vs_data(vs_input, data);

	// add offset by anchor. Anchor is a normalized value
	// float4x4 offset_m = create_translate((data.scale * data.anchor) * float2(-1, -1));
	// float4x4 scale_m = create_scale(data.scale);
	// float4x4 transform = mul(scale_m, offset_m);
	// transform = mul(transform, create_rotation(data.cos_and_sin));
	// transform = mul(transform, create_translate(data.position));

	float4x4 transform = MatrixFromRows(
		vs_input.row0,
		vs_input.row1,
		vs_input.row2,
		vs_input.row3);
	
	float4 vertex = float4(vertices[vs_input.vertex_id], 0.0, 1.0);
	// float4 pos = mul(mul(g_projection, transpose(transform)), vertex);
	float4 pos = mul(vertex, transform);
	
	ps_input.pos = pos;
	ps_input.uv = uvs[vs_input.vertex_id];
	ps_input.color = g_color;
}