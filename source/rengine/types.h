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
    
    typedef void(*action_t)();

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
        typedef u32 pipeline_state_t;
        typedef u32 srb_t;
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
        static u16 no_srb                 = MAX_U16_VALUE;
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
            colorf = 1 << 3,
            uv = 1 << 4,
            tangent = 1 << 5,
            instancing = 1 << 6
        };

        enum class resource_usage : u8 {
            // after created, can not be modified anymore
            immutable = 0,
			// can be modified, but not frequently (low frequency updates)
			normal,
			// can be modified frequently (idealy for high frequency updates)
            dynamic
        };

        enum class resource_type : u8 {
            unknow = 0,
            tex2d,
            tex3d,
            texcube,
            texarray,
            rt,
            cbuffer,
        };

        enum class texture_format : u32 {
			unknown = 0,
			// color formats
			rgba8,
			bgra8,
            rgba8_srgb,
			bgra8_srgb,
            // hdr color formats
			rgba16f,
            rgba32f,
            // depth + stencil formats
            d16,
			d24s8,
			d32s8,
			d32f,
			r8,
            rg8,
            // compressed formats
            bc1_dxt1,
            bc3_dxt5,
            bc4,
            bc5,
            bc6h,
            bc7,
        };

        // TODO: add more types
        enum class shader_type : u8 {
            vertex = 0,
            pixel,
            max,
        };

        enum class shader_type_flags : u32 {
            none = 0,
            vertex = 1 << 0,
			pixel = 1 << 1,
        };

        enum class filter_type : u8 {
            unknown = 0,                    ///< Unknown filter type
            point,                          ///< Point filtering
            linear,                         ///< Linear filtering
            anisotropic,                    ///< Anisotropic filtering
            comparison_point,              ///< Comparison-point filtering
            comparison_linear,             ///< Comparison-linear filtering
            comparison_anisotropic,        ///< Comparison-anisotropic filtering
            minimum_point,                 ///< Minimum-point filtering (DX12 only)
            minimum_linear,                ///< Minimum-linear filtering (DX12 only)
            minimum_anisotropic,           ///< Minimum-anisotropic filtering (DX12 only)
            maximum_point,                 ///< Maximum-point filtering (DX12 only)
            maximum_linear,                ///< Maximum-linear filtering (DX12 only)
            maximum_anisotropic,           ///< Maximum-anisotropic filtering (DX12 only)
        };

        enum class texture_address_mode : u8 {
            unknown = 0,               ///< Unknown mode

            /// Tile the texture at every integer junction.
            /// Direct3D: D3D11_TEXTURE_ADDRESS_WRAP/D3D12_TEXTURE_ADDRESS_MODE_WRAP
            /// OpenGL: GL_REPEAT
            wrap = 1,

            /// Flip the texture at every integer junction.
            /// Direct3D: D3D11_TEXTURE_ADDRESS_MIRROR/D3D12_TEXTURE_ADDRESS_MODE_MIRROR
            /// OpenGL: GL_MIRRORED_REPEAT
            mirror = 2,

            /// Clamp texture coordinates outside [0.0, 1.0] to the edge color.
            /// Direct3D: D3D11_TEXTURE_ADDRESS_CLAMP/D3D12_TEXTURE_ADDRESS_MODE_CLAMP
            /// OpenGL: GL_CLAMP_TO_EDGE
            clamp = 3,

            /// Clamp texture coordinates outside [0.0, 1.0] to the border color.
            /// Direct3D: D3D11_TEXTURE_ADDRESS_BORDER/D3D12_TEXTURE_ADDRESS_MODE_BORDER
            /// OpenGL: GL_CLAMP_TO_BORDER
            border = 4,

            /// Take absolute value of texture coordinate and then clamp.
            /// Direct3D: D3D11_TEXTURE_ADDRESS_MIRROR_ONCE/D3D12_TEXTURE_ADDRESS_MODE_MIRROR_ONCE
            /// OpenGL: GL_MIRROR_CLAMP_TO_EDGE (GL4.4+ / GLES3.1+)
            mirror_once = 5,
        };

        enum class comparison_function : u8 {
            unknown = 0,             ///< Unknown comparison function

            /// Comparison never passes.
            /// Direct3D: D3D11_COMPARISON_NEVER/D3D12_COMPARISON_FUNC_NEVER
            /// OpenGL: GL_NEVER
            never,

            /// Passes if source < destination.
            /// Direct3D: D3D11_COMPARISON_LESS/D3D12_COMPARISON_FUNC_LESS
            /// OpenGL: GL_LESS
            less,

            /// Passes if source == destination.
            /// Direct3D: D3D11_COMPARISON_EQUAL/D3D12_COMPARISON_FUNC_EQUAL
            /// OpenGL: GL_EQUAL
            equal,

            /// Passes if source <= destination.
            /// Direct3D: D3D11_COMPARISON_LESS_EQUAL/D3D12_COMPARISON_FUNC_LESS_EQUAL
            /// OpenGL: GL_LEQUAL
            less_equal,

            /// Passes if source > destination.
            /// Direct3D: D3D11_COMPARISON_GREATER/D3D12_COMPARISON_FUNC_GREATER
            /// OpenGL: GL_GREATER
            greater,

            /// Passes if source != destination.
            /// Direct3D: D3D11_COMPARISON_NOT_EQUAL/D3D12_COMPARISON_FUNC_NOT_EQUAL
            /// OpenGL: GL_NOTEQUAL
            not_equal,

            /// Passes if source >= destination.
            /// Direct3D: D3D11_COMPARISON_GREATER_EQUAL/D3D12_COMPARISON_FUNC_GREATER_EQUAL
            /// OpenGL: GL_GEQUAL
            greater_equal,

            /// Comparison always passes.
            /// Direct3D: D3D11_COMPARISON_ALWAYS/D3D12_COMPARISON_FUNC_ALWAYS
            /// OpenGL: GL_ALWAYS
            always,
        };
    }

    namespace math {
        static number_t pi = 3.14159265358979323846264338327950288;
        static number_t half_pi = pi * 0.5;
        static number_t deg_2_rad_ratio = pi / 180.;
        static number_t rad_2_deg_ratio = 1. / deg_2_rad_ratio;
    }
}