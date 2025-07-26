#ifndef MATERIALS_MODULE
#define MATERIALS_MODULE 1

#include "_types.module.slang"
#include "_utils.module.slang"

namespace rengine {
    namespace materials {
        namespace unlit {
            public struct UnlitVertexInput: IVertexInput {
                float3 position : POSITION_ATTR;
                uint color      : COLOR_ATTR;

                public define_vs_layout(VertexLayout.position | VertexLayout.color);
                public define_vs_get_position(position);
                public define_vs_get_color(color);

                public define_vec2_empty_fn(getUV0);
                public define_vec2_empty_fn(getUV1);
                public define_vec3_empty_fn(getNormal);
                public define_vec3_empty_fn(getTangent);
                public define_vec3_empty_fn(getBiTangent);
            }

            public struct UnlitTextureVertexInput: IVertexInput {
                float3 position : POSITION_ATTR;
                uint color      : COLOR_ATTR;
                float2 uv       : UV0_ATTR;

                public define_vs_layout(VertexLayout.position | VertexLayout.color | VertexLayout.uv0);
                public define_vs_get_position(position);
                public define_vs_get_color(to_rgba(color));
                public define_vs_get_uv(uv, 0);
                public define_vs_get_uv(uv, 1);

                public define_vec3_empty_fn(getNormal);
                public define_vec3_empty_fn(getTangent);
                public define_vec3_empty_fn(getBiTangent);
            }

            public struct UnlitMaterial: IMaterial {
                float4 _albedo;

                __init(in StageInput input, ITexture2D tex) {
                    _albedo = input.color * tex.sample(input.uv);
                }

                public float4 getAlbedo() { return _albedo; }
                public float3 getNormal() { return float3(0); }
            }
        }
    }
}

#endif