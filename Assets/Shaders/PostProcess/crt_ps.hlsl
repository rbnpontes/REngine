// Heavily inspired on https://www.shadertoy.com/view/Ms23DR
Texture2D g_texture;
SamplerState g_texture_sampler;

struct PSInput
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float2 screenSize : TEXCOORD1;
    float4 worldPos : TEXCOORD2;
    float2 time : TEXCOORD3;
};

// distort UV coords to appear like old CRT TV
float2 curve_uv(float2 uv, float scale, float distortion, float radius, float scale_multiplier)
{
    // convert coords from [0 ~ 1] to [-1 ~ 1]
    uv = (uv - 0.5) * 2.0;
    // scale uv
    uv *= scale;
    // apply curvature and distortion
    uv.x *= 1.0 + pow((abs(uv.y) / distortion), radius);
    uv.y *= 1.0 + pow((abs(uv.x) / distortion), radius);
    // now, revert coords from [-1 ~ 1] to [0 ~ 1]
    uv = (uv / 2.0) + 0.5;
    uv = uv * scale_multiplier + 0.04;
    return uv;
}

// generate an simple pseudo random wave. this effect will be usefull
// to generate screen glitches
float wave(float t, float amplitude, float3 freq_weight, float3 offset_weight)
{
    return (sin(offset_weight.x + t * freq_weight.x)
        * sin(offset_weight.y + t * freq_weight.y)
        * sin(offset_weight.z + t * freq_weight.z))*amplitude;
}

float3 chromatic_aberration(float2 uv, float3 offsets)
{
    float3 color = float3(
        g_texture.Sample(g_texture_sampler, float2(uv.x + offsets.x, uv.y + offsets.x)).r,
        g_texture.Sample(g_texture_sampler, float2(uv.x + offsets.z, uv.y - offsets.y)).g,
        g_texture.Sample(g_texture_sampler, float2(uv.x - offsets.y, uv.y + offsets.z)).b
    );
    return color;
}

// apply ghost artifacts on screen
float3 ghost_effect(float2 uv, float uv_scale, float3 threshold, float3 offsets)
{
    float3 color = float3(
        threshold.r * g_texture.Sample(g_texture_sampler, uv_scale * (uv + offsets)).r,
        threshold.g * g_texture.Sample(g_texture_sampler, uv_scale * (uv - offsets)).g,
        threshold.b * g_texture.Sample(g_texture_sampler, uv_scale * (uv + offsets)).b
    );
    return color;
}

float3 scans_effect(float3 color, float2 uv, float t)
{
	const float scans = clamp(0.35 + 0.35 * sin(3.5 * t + uv.y * 1.5), 0.0, 1.0);

    float s = pow(scans, 0.1);
    s = 0.4 + 0.7 * s;

    color *= float3(s, s, s);
    color *= 1.0 + 0.01 * sin(110.0 * t);

    return color;
}


float3 crt_effect(float3 color, float uv_x, float screen_width)
{
    const float x = fmod(uv_x * screen_width, 2.0);
    const float crt = clamp((x - 1.0) * 2.0, 0.0, 1.0);
    return color * 1.0 - 0.65 * float3(crt, crt, crt);
}

float4 main(in PSInput ps_input) : SV_TARGET
{
    float2 uv = curve_uv(ps_input.uv, 1.1f, 4.48, 2.0, 0.92);
#ifdef GLSL
    uv.y = 1.0f - uv.y;
#endif
    
    const float t = wave(ps_input.time.x, 0.00017, float3(6.3, 20.3, 10.23), float3(0, 0, 0.3));

    float3 color = chromatic_aberration(uv + t, float3(0.001, 0.002, 0.0));
	color += ghost_effect(uv + t, 0.98, float3(0.08, 0.05, 0.08), float3(0.0028, 0.02, 0.0018));
    color *= scans_effect(color, uv, ps_input.time.x * 0.001);
    color = crt_effect(color, ps_input.uv.x, ps_input.screenSize.x);

    if (uv.x < 0.0 || uv.x > 1.0)
        color *= 0.0;
    if (uv.y < 0.0 || uv.y > 1.0)
        color *= 0.0;

	return float4(color * 1.1, 1.0);
}