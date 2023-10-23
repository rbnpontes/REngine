Texture2D g_texture;
SamplerState g_texture_sampler;

#define SMOOTHING_OFFSET 0.503
#define SHADOW_OFFSET 0

struct PSInput
{
    float4 pos : SV_Position;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    float fontScale : TEXCOORD1;
};

float4 main(in PSInput input) : SV_Target
{
    float smoothing = input.fontScale;
    float d = g_texture.Sample(g_texture_sampler, input.uv).r;
    float alpha = smoothstep(SMOOTHING_OFFSET - smoothing, SMOOTHING_OFFSET + smoothing, d);
    
    return input.color * float4(1.0, 1.0, 1.0, alpha);
}