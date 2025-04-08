#pragma once
#include "../base_private.h"
#include "./pipeline_state_manager.h"

#include "../math/math-types.h"
#include "../io/logger.h"

#include <GraphicsTypes.h>
#include <DeviceContext.h>

namespace rengine {
    namespace graphics {
        enum class renderer_dirty_flags : u32 {
            none            = 0,
            render_targets  = 1 << 0,
            depth_stencil   = 1 << 1,
            vertex_buffer   = 1 << 2,
            index_buffer    = 1 << 3,
            viewport        = 1 << 4
        };

       /* struct render_command_data {
			pipeline_state_t pipeline_state { no_pipeline_state };
			shader_t vertex_shader { no_shader };
			shader_t pixel_shader { no_shader };
            texture_
			vertex_buffer_t vertex_buffers[GRAPHICS_MAX_VBUFFERS]{};
        };*/

        struct renderer_state {
            io::ILog* log{ null };
            array<render_target_t, GRAPHICS_MAX_RENDER_TARGETS> render_targets{};
            render_target_t depth_stencil{ no_render_target };
            u32 dirty_flags { (u32)renderer_dirty_flags::none };
            math::urect viewport { math::urect::zero };
            u8 num_render_targets{ 0 };
        };

        extern renderer_state g_renderer_state;

        void renderer__init();
        void renderer__deinit();
        void renderer__reset_state();
        void renderer__set_render_targets();
        void renderer__set_viewport();
        void renderer__submit_render_state();
        void renderer__assert_render_target_idx(u8 idx);
    }
}