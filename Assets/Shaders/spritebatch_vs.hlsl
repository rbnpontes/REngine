cbuffer ObjectConstants
{
	float4x4 g_model;
}
cbuffer FrameConstants
{
    float4x4 g_projection;
};

struct vs_input
{
	uint vertex_id : SV_VertexID;
	float4 row0 : ATTRIB0;
	float4 row1 : ATTRIB1;
	float4 row2 : ATTRIB2;
	float4 row3 : ATTRIB3;
};

struct PSOutput {
	float4 pos		: SV_POSITION;
	float2 uv		: TEXCOORD0;
	float4 color	: COLOR0;
};

#define IDENTITY_MATRIX MatrixFromRows(float4(1,0,0,0), float4(0,1,0,0), float4(0,0,1,0), float4(0,0,0,1))

float4x4 create_scale(float2 scale)
{
	float4x4 result = IDENTITY_MATRIX;
	MATRIX_ELEMENT(result, 0, 0) = scale.x;
	MATRIX_ELEMENT(result, 1, 1) = scale.y;
	return result;
}
float4x4 create_rotation(float cosAngle, float sinAngle)
{
	float4x4 result = IDENTITY_MATRIX;
	MATRIX_ELEMENT(result, 0, 0) = cosAngle;
	MATRIX_ELEMENT(result, 0, 1) = sinAngle;
	MATRIX_ELEMENT(result, 1, 0) = -sinAngle;
	MATRIX_ELEMENT(result, 1, 1) = cosAngle;
	return result;
}
float4x4 create_translate(float3 position)
{
	float4x4 result = IDENTITY_MATRIX;
	MATRIX_ELEMENT(result, 3, 0) = position.x;
	MATRIX_ELEMENT(result, 3, 1) = position.y;
	MATRIX_ELEMENT(result, 3, 2) = position.z;
	return result;
}

float3 get_position(vs_input data)
{
	return float3(data.row0.x, data.row0.y, data.row0.z);
}
float2 get_rotation_coefficients(vs_input data)
{
	return float2(data.row0.w, data.row1.x);
}
float2 get_scale(vs_input data)
{
	return float2(data.row1.y, data.row1.z);
}
float2 get_anchor(vs_input data)
{
	return float2(data.row1.w, data.row2.x);
}
float4 get_color(vs_input data)
{
	return float4(
		data.row2.y,
		data.row2.z,
		data.row2.w,
		data.row3.x
	);
}

float4x4 get_transform(vs_input data)
{
	float3 position = get_position(data);
	float2 coefficients = get_rotation_coefficients(data);
	float2 scale = get_scale(data);

	float4x4 transform = mul(create_scale(scale), create_rotation(coefficients.x, coefficients.y));
	transform = mul(transform, create_translate(position));
	return mul(g_projection, transpose(transform));
}

void main(in vs_input vs_data, out PSOutput ps_input) 
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
	
	float2 vpos = vertices[vs_data.vertex_id] + get_anchor(vs_data);
	float4x4 transform = get_transform(vs_data);
	transform = mul(transform, g_model);
	
	float4 pos = mul(transform, float4(vpos, 0.0, 1.0));
	ps_input.pos = pos;
	ps_input.uv = uvs[vs_data.vertex_id];
	ps_input.color = get_color(vs_data);
}