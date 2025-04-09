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
    typedef void* ptr;

    #ifdef HIGH_DEFINITION_PRECISION
        typedef long long int_t;
        typedef unsigned long long uint_t;
        typedef double number_t;
    #else
        typedef int int_t;
        typedef unsigned int uint_t;
        typedef float number_t;
    #endif

    #define null nullptr
    
    namespace core {
        typedef u32 window_t;
        static u32 no_window = MAX_U32_VALUE;
        typedef u32 hash_t;
    }

    namespace graphics {
        // buffer objects
        typedef u16 vertex_buffer_t;
        typedef u16 index_buffer_t;
        typedef u16 constant_buffer_t;
        typedef u16 instancing_buffer_t;
        // texture objects
        typedef u16 texture_2d_t;
        typedef u16 texture_3d_t;
        typedef u16 texture_cube_t;
        typedef u16 texture_array_t;
        typedef u16 render_target_t;
        typedef u16 pipeline_state_t;
        typedef u16 srb_t;
        // shader objects
        typedef u32 shader_t;
        typedef u32 shader_program_t;
        // render objects
		typedef u32 render_command_t;
        typedef u16 material_t;
        typedef u16 mesh_t;
        typedef u16 model_t;
        typedef u8 animated_model_t;
        typedef u8 camera_t;
        typedef LIGHT_ENTITY_SIZE light_t;

        static u16 no_vertex_buffer       = MAX_U16_VALUE;
        static u16 no_index_buffer        = MAX_U16_VALUE;
        static u16 no_constant_buffer     = MAX_U8_VALUE;
        static u8 no_instancing_buffer    = MAX_U8_VALUE;
        static u8 no_texture_2d           = MAX_U8_VALUE;
        static u8 no_texture_3d           = MAX_U8_VALUE;
        static u8 no_texture_cube         = MAX_U8_VALUE;
        static u8 no_texture_array        = MAX_U8_VALUE;
        static u8 no_render_target        = MAX_U8_VALUE;
        static u16 no_pipeline_state      = MAX_U16_VALUE;
        static u16 no_srb_state           = MAX_U16_VALUE;
        static u32 no_shader              = MAX_U32_VALUE;
        static u32 no_shader_program      = MAX_U32_VALUE;
		static u32 no_render_command      = 0;
        static u16 no_material            = MAX_U16_VALUE;
        static u16 no_mesh                = MAX_U16_VALUE;
        static u16 no_model               = MAX_U16_VALUE;
        static u8 no_animated_model       = MAX_U8_VALUE;
        static u8 no_camera               = MAX_U8_VALUE;
        static LIGHT_ENTITY_SIZE no_light = -1;

        enum class backend : byte {
            d3d11 = 0,
            d3d12,
            vulkan,
            webgpu,
            opengl,
            max_backend
        };

        enum class primitive_topology : u8 {
            triangle_list = 0,
            triangle_strip,
            point_list,
            line_list,
            line_strip
        };

        enum class cull_mode : u8 {
            none = 0,
            counter_clock_wise,
            clock_wise
        };

        enum class vertex_elements : u32 {
            none = 0,
            position = 1 << 0,
            normal = 1 << 1,
            color = 1 << 2,
            texcoord = 1 << 3,
            tangent = 1 << 4,
            instancing = 1 << 5
        };

        // TODO: add more types
        enum class shader_type : u8 {
            vertex = 0,
            pixel,
        };
    }
}