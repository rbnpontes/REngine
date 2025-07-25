#ifndef UTILS_MODULE
#define UTILS_MODULE 1
#include "_base.module.slang"

#define sample_tex(tex, coords) tex.Sample(tex##_sampler, coords)
#define tex2d(tex, uv) sample_tex(tex, uv)

#define import_tex(tex_type, uv_type, ret_type, name)               \
    tex_type<ret_type> name;                                        \
    SamplerState name##_sampler;                                    \
    struct name##_TextureSampler: ITexture<uv_type, ret_type> {     \
        ret_type sample(uv_type coords) {                           \
            return name.Sample(name##_sampler, coords);   \
        }                                                           \
    }

#define import_tex2d(name) import_tex(Texture2D, float2, float4, name)
#define import_tex3d(name) import_tex(Texture3D, float3, float4, name)
#define import_texcube(name) import_tex(TextureCube, float3, float4, name)

#define use_texture(name) name##_TextureSampler()

// Fast declaration methods helper macros
#define define_vec2_fn(name, accessor) \
    float2 name() { return accessor; }
#define define_vec3_fn(name, accessor) \
    float3 name() { return accessor; }
#define define_vec4_fn(name, accessor) \
    float4 name() { return accessor; }

#define define_vec2_empty_fn(name) \
    float2 name() { return float2(0); }
#define define_vec3_empty_fn(name) \
    float3 name() { return float3(0); }
#define define_vec4_empty_fn(name) \
    float4 name() { return float4(0); }

#define define_vs_layout(flags) \
    VertexLayout getLayout() { \
        return flags; \
    }
#define define_vs_get_position(accessor) \
    define_vec3_fn(getPosition, accessor)
#define define_vs_get_color(accessor) \
    define_vec4_fn(getColor, accessor)
#define define_vs_get_uv(accessor, idx) \
    define_vec2_fn(getUV##idx, accessor)

#define to_rgba(uint_color) float4( \
    (float)((color >> 0) & 0xFF), \
    (float)((color >> 8) & 0xFF), \
    (float)((color >> 16) & 0xFF), \
    (float)((color >> 24) & 0xFF) \
);


static const float4x4 s_matrix4x4_identity = float4x4(
     1, 0, 0, 0,
     0, 1, 0, 0,
     0, 0, 1, 0,
     0, 0, 0, 1
);
#endif