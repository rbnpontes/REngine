struct ps_input {
    float4 position : SV_Position;
    float4 color    : COLOR0;
    float2 uv       : TEXCOORD0;
};
float4 main(in ps_input input) : SV_Target {
    return input.color;
}
