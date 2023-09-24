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
	float4 positionAndScale		: ATTRIB2;
	float4 rotationAndAnchor	: ATTRIB3;
};
#else
struct PSInput {
	float2 pos : ATTRIB0;
	float2 uv : ATTRIB1;
};
#endif


#define IDENTITY_MATRIX float4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)

float4x4 createScale(float2 scale) {
	float4x4 result = IDENTITY_MATRIX;
	result[0][0] = scale.x;
	result[1][1] = scale.y;
	return result;
}
float4x4 createRotation(float angle) {
	float4x4 result = IDENTITY_MATRIX;
	float c = cos(angle);
	float s = sin(angle);

	result[0][0] = c;
	result[0][1] = s;
	result[1][0] = -s;
	result[1][1] = c;

	return result;
}
float4x4 createTranslate(float2 position) {
	float4x4 result = IDENTITY_MATRIX;

	result[3][0] = position.x;
	result[3][1] = position.y;

	return result;
}


struct PSOutput {
	float4 pos		: SV_POSITION;
	float2 uv		: TEXCOORD0;
	float4 color	: COLOR0;
};
void main(in PSInput input, out PSOutput output) 
{
#ifdef RENGINE_INSTANCED
	float2 position = float2(input.positionAndScale.x, input.positionAndScale.y);
	float2 scale = float2(input.positionAndScale.z, input.positionAndScale.w);
	float2 anchor = float2(input.rotationAndAnchor.x, input.rotationAndAnchor.y);
	float rotation = input.rotationAndAnchor.z;

	float4x4 transform = mul(createScale(scale), createTranslate((scale * anchor) * float2(-1, -1)));
	transform = mul(transform, createRotation(rotation));
	transform = mul(transform, createTranslate(position));
	float4 pos = mul(mul(g_projection, transpose(transform)), float4(input.pos, 0.0, 1.0));
#else
	float4 pos = mul(mul(g_projection, g_transform), float4(input.pos, 0.0, 1.0));
#endif
	output.pos = pos;
	output.uv = input.uv;
	output.color = g_color;
}