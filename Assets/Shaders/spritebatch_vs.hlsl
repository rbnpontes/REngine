﻿cbuffer ObjectConstants {
	float4x4 g_transform;
	float4 g_color;
};
cbuffer FrameConstants
{
    float4x4 g_projection;
};

struct PSInput {
	float2 pos : ATTRIB0;
	float2 uv : ATTRIB1;
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
void main(in PSInput vs_input, out PSOutput ps_input) 
{
	float4 pos = mul(mul(g_projection, g_transform), float4(vs_input.pos, 0.0, 1.0));
	ps_input.pos = pos;
	ps_input.uv = vs_input.uv;
	ps_input.color = g_color;
}