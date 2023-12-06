Texture2D g_texture;
SamplerState g_texture_sampler;

struct PSInput
{
    float4 pos      : SV_Position;
    float4 color    : COLOR;
    float2 uv       : TEXCOORD;
};

float4 main(in PSInput ps_input) : SV_Target
{
    return ps_input.color * g_texture.Sample(g_texture_sampler, ps_input.uv);
}