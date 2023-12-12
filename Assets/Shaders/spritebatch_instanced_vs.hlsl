cbuffer ObjectConstants {
	float4 g_color;
};

cbuffer FrameConstants
{
    float4x4 g_projection;
};

struct PSInput {
	float2 pos : ATTRIB0;
	float2 uv : ATTRIB1;
	// Instance values
	float4 positionAndScale		: ATTRIB2;
	float4 rotationAndAnchor	: ATTRIB3;
};


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
float4x4 createTranslate(float2 position) {
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

struct PSOutput {
	float4 pos		: SV_POSITION;
	float2 uv		: TEXCOORD0;
	float4 color	: COLOR0;
};
void main(in PSInput vs_input, out PSOutput ps_input) 
{
	float2 position = vs_input.positionAndScale.xy;
	float2 scale = vs_input.positionAndScale.zw;
	float2 anchor = vs_input.rotationAndAnchor.xy;
	float rotation = vs_input.rotationAndAnchor.z;

	float4x4 translate_m = createTranslate((scale * anchor) * float2(-1, -1));
	float4x4 scale_m = createScale(scale);
	
	float4x4 transform = mul(scale_m, translate_m);
	transform = mul(transform, createRotation(rotation));
	transform = mul(transform, createTranslate(position));
	float4 pos = mul(mul(g_projection, transpose(transform)), float4(vs_input.pos, 0.0, 1.0));

	// float4x4 transform = translate_m * scale_m;
	// transform = transform * createTranslate(position);
	// float4 pos = g_projection * transform * float4(vs_input.pos, 0.0, 1.0);
	
	ps_input.pos = pos;
	ps_input.uv = vs_input.uv;
	ps_input.color = g_color;
}