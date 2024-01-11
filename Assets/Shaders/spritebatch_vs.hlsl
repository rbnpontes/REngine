// Transform Data is encoded
cbuffer ObjectConstants {
	float4x4 g_data;
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

float get_element(int x, int y)
{
	return MATRIX_ELEMENT(g_data, x, y);
}

float3 get_position()
{
	return float3(get_element(0, 0), get_element(1, 0), get_element(2, 0));
}
float2 get_rotation_coefficients()
{
	return float2(get_element(3, 0), get_element(0, 1));
}
float2 get_scale()
{
	return float2(get_element(1, 1), get_element(2, 1));
}
float2 get_anchor()
{
	return float2(get_element(3,1), get_element(0, 2));
}
float4 get_color()
{
	return float4(
		get_element(1, 2),
		get_element(2, 2),
		get_element(3, 2),
		get_element(0, 3)
	);
}

float4x4 get_transform()
{
	float3 position = get_position();
	float2 coefficients = get_rotation_coefficients();
	float2 scale = float2(get_element(1, 1), get_element(2, 1));

	float4x4 transform = mul(create_scale(scale), create_rotation(coefficients.x, coefficients.y));
	transform = mul(transform, create_translate(position));
	return mul(g_projection, transpose(transform));
}

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
	
	float2 vpos = vertices[vs_input.vertex_id] + get_anchor();
	float4x4 transform = get_transform();
	float4 pos = mul(transform, float4(vpos, 0.0, 1.0));
	ps_input.pos = pos;
	ps_input.uv = uvs[vs_input.vertex_id];
	ps_input.color = get_color();
}