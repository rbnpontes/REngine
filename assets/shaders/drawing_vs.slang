cbuffer frame_constants {
    float4x4 g_screen_projection;
    float2 g_window_size;
    float g_delta_time;
    float g_elapsed_time;
    uint g_frame;
};

struct vs_input {
#if defined(POSITION_ATTR)
    float3 position : POSITION_ATTR;
#endif
#if defined(COLOR_ATTR)
    uint color      : COLOR_ATTR;
#endif
#if defined(COLORF_ATTR)
    float4 colorf   : COLORF_ATTR;
#endif
#if defined(UV_ATTR)
    float2 uv           : UV_ATTR;
#endif
};

struct vs_output {
    float4 position : SV_Position;
    float4 color    : COLOR0;
    float2 uv       : TEXCOORD0;
};

vs_output main(in vs_input input) {
    vs_output output = (vs_output)0;

#if defined(POSITION_ATTR)
    float4 vertex_position = float4(input.position, 1.0f);
#else
    float4 vertex_position = float4(0.0f, 0.0f, 0.0f, 1.0f);
#endif

#if defined(COLORF_ATTR)
    float4 color_float = input.colorf;
#else
    float4 color_float = float4(0, 0, 0, 1);
#endif

#if defined(COLOR_ATTR)
    uint color_int = input.color;
#else
    uint color_int = 0;
#endif
    float4 color = float4(
        (float)((color_int >> 0) & 0xFF),
        (float)((color_int >> 8) & 0xFF),
        (float)((color_int >> 16) & 0xFF),
        (float)((color_int >> 24) & 0xFF)
    );
    color /= float4(255.0f, 255.0f, 255.0f, 255.0f);

#if defined(UV_ATTR)
    float2 uv = input.uv;
#else
    float2 uv = float2(0.0f, 0.0f);
#endif

    output.position = mul(g_screen_projection, vertex_position);
    output.color = color + color_float;
    output.color.a *= 0.5;
    output.uv = uv;
    return output;
}
