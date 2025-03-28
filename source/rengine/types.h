#pragma once
#include "./defines.h"

#include <stdint.h>

namespace rengine {
    // default primitives
    typedef char i8;
    typedef short i16;
    typedef int i32;
    typedef long long i64;
    typedef unsigned char u8;
    typedef unsigned short u16;
    typedef unsigned int u32;
    typedef unsigned long long u64;
    // data primitives
    typedef const char* c_str;
    typedef unsigned char byte;
    typedef unsigned short entity;

    #ifdef HIGH_DEFINITION_PRECISION
        typedef int int_t;
        typedef unsigned int uint_t;
        typedef float number_t;
    #else
        typedef long long int_t;
        typedef unsigned long long uint_t;
        typedef double number_t;
    #endif

    #define null nullptr
    
    namespace core {
        typedef u32 window_t;
        static u32 no_window = MAX_U32_VALUE;
    }

    namespace graphics {
        // buffer objects
        struct vertex_buffer_t;
        struct index_buffer_t;
        struct constant_buffer_t;
        struct instancing_buffer_t;
        // texture objects
        struct texture_2d_t;
        struct texture_3d_t;
        struct texture_cube_t;
        struct texture_array_t;
        struct render_target_t;
        // shader objects
        struct shader_t;
        struct shader_program_t;
        // render objects
        struct material_t;
        struct mesh_t;
        struct model_t;
        struct animatedModel_t;
        struct camera_t;
        struct light_t;

        enum class backend : byte {
            d3d11 = 0,
            d3d12,
            vulkan,
            webgpu,
            opengl,
            max_backend
        };
    }
}