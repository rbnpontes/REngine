Texture2D    g_texture;
SamplerState g_texture_sampler;

struct ps_input {
    float4 position : SV_Position;
    float4 color    : COLOR0;
    float2 uv           : TEXCOORD0;
};
float4 main(in ps_input input) : SV_Target {
    return input.color * g_texture.Sample(g_texture_sampler, input.uv);
}
