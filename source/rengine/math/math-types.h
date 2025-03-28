#pragma once
#include <rengine/types.h>

namespace rengine {
    namespace math {
        template<typename T>
        struct base_vec2_t {
            T x, y;
        };
        typedef base_vec2_t<number_t> vec2_t;
        typedef base_vec2_t<int_t> ivec2_t;
        typedef base_vec2_t<uint_t> uvec2_t;

        template<typename T>
        struct base_vec3_t {
            T x, y, z;
        };
        typedef base_vec3_t<number_t> vec3_t;
        typedef base_vec3_t<int_t> ivec3_t;
        typedef base_vec3_t<uint_t> uvec3_t;

        template<typename T>
        struct base_vec4_t {
            T x, y, z, w;
        };
        typedef base_vec4_t<number_t> vec4_t;
        typedef base_vec4_t<int_t> ivec4_t;
        typedef base_vec4_t<uint_t> uvec4_t;

        struct quat_t {
            number_t x, y, z, w;
        };

        template<typename T>
        struct base_rect_t {
            T position, size;
        };
        typedef base_rect_t<vec2_t> rect_t;
        typedef base_rect_t<ivec2_t> irect_t;
        typedef base_rect_t<uvec2_t> urect_t;

        struct box {
            vec3_t min, max;
        };
    }
}