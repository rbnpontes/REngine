﻿cbuffer FixedConstants
{
	float4x4 g_projection;
};
cbuffer ObjectConstants
{
    float4 g_color;
    float4 g_positionAndSizes;
};

struct VSInput
{
    uint vertexId : SV_VertexID;
	float4 bounds : ATTRIB0;
    float4 positionAndAtlasSize : ATTRIB1;
};

struct PSOutput
{
	float4 pos : SV_POSITION;
	float4 color : COLOR;
	float2 uv : TEXCOORD0;
    float fontScale : TEXCOORD1;
};

void main(in VSInput input, out PSOutput output)
{
    float2 atlasSize = input.positionAndAtlasSize.zw;
    float4 normalizedBounds = float4(
        input.bounds.x / atlasSize.x,
        input.bounds.y / atlasSize.y,
        input.bounds.z / atlasSize.x,
        input.bounds.w / atlasSize.y
    );
    float2 min = input.positionAndAtlasSize.xy;
    float2 max = min + (input.bounds.zw - input.bounds.xy);
    
    float2 posValues[4];
    posValues[0] = float2(min.x, max.y); // (0, 1)
    posValues[1] = float2(min.x, min.y); // (0, 0)
    posValues[2] = float2(max.x, max.y); // (1, 1)
    posValues[3] = float2(max.x, min.y); // (1, 0)
    float2 uvValues[4];
    uvValues[0] = float2(normalizedBounds.x, normalizedBounds.w); // (0, 0)
    uvValues[1] = float2(normalizedBounds.x, normalizedBounds.y); // (0, 1)
    uvValues[2] = float2(normalizedBounds.z, normalizedBounds.w); // (1, 0)
    uvValues[3] = float2(normalizedBounds.z, normalizedBounds.y); // (1, 1)
	
    float2 position = posValues[input.vertexId];
    float2 uv = uvValues[input.vertexId];
    
    // char sizes is relative to atlas size
    // in this case, we must scale down and apply final font size
    position *= 1.0f / g_positionAndSizes.w;
    position *= g_positionAndSizes.z;
    position += g_positionAndSizes.xy; // apply position
    
    output.fontScale = 1.0f / g_positionAndSizes.z;
    output.pos = mul(g_projection, float4(position, 0.0, 1.0));
	output.color = g_color;
	output.uv = uv;
}