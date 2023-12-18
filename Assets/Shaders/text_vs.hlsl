cbuffer FrameConstants
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

void main(in VSInput vs_input, out PSOutput ps_input)
{
    float2 atlasSize = vs_input.positionAndAtlasSize.zw;
    float4 normalizedBounds = float4(
        vs_input.bounds.x / atlasSize.x,
        vs_input.bounds.y / atlasSize.y,
        vs_input.bounds.z / atlasSize.x,
        vs_input.bounds.w / atlasSize.y
    );
    float2 min = vs_input.positionAndAtlasSize.xy;
    float2 max = min + (vs_input.bounds.zw - vs_input.bounds.xy);
    
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
	
    float2 position = posValues[vs_input.vertexId];
    float2 uv = uvValues[vs_input.vertexId];
    
    // char sizes is relative to atlas size
    // in this case, we must scale down and apply final font size
    position *= 1.0f / g_positionAndSizes.w;
    position *= g_positionAndSizes.z;
    
    ps_input.fontScale = 1.0f / g_positionAndSizes.z;
    ps_input.pos = mul(g_projection, float4(position + g_positionAndSizes.xy, 0.0, 1.0));
	ps_input.color = g_color;
	ps_input.uv = uv;
}