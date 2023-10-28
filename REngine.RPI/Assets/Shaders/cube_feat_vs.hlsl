cbuffer CameraConstants
{
    float4x4 g_camView;
    float4x4 g_camViewInverse;
    float4x4 g_camViewProjection;
    float4 g_camPosition;
    float g_camNearClip;
    float g_camFarClip;
};

cbuffer ObjectConstants
{
	float4x4 g_model;
};

struct VSInput
{
	float3 pos		: ATTRIB0;
	float4 color	: ATTRIB1;
	//float2 uv		: ATTRIB1;
};

struct PSInput
{
	float4 pos		: SV_POSITION;
	float4 color	: COLOR;
	//float2 uv		: TEXCOORD;
};

void main(in VSInput input, out PSInput output)
{
    //float4x4 mvp = mul(mul(transpose(g_model), transpose(g_camView)), transpose(g_camViewProjection));
    float4x4 mvp = transpose(mul(mul(g_camViewProjection, g_camView), g_model));
    //float4x4 mvp = g_model * g_camView * g_camViewProjection;
    //float4x4 mvp = g_camViewProjection * g_camView * g_model;
    //output.pos = mul(float4(input.pos, 1.0), transpose(mvp));
    output.pos = mul(float4(input.pos, 1.0), mvp);
	output.color = input.color;
	//output.uv = input.uv;
}