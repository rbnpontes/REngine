#pragma once
#include "../base_private.h"
#include "./pipeline_state_manager.h"
#include "./render_command_private.h"

#include "../math/math-types.h"
#include "../io/logger.h"

#include <GraphicsTypes.h>
#include <DeviceContext.h>

namespace rengine {
    namespace graphics {
        enum class renderer_dirty_flags : u32 {
            none            = 0,
            render_targets  = 1 << 0,
            vertex_buffer   = 1 << 1,
            index_buffer    = 1 << 2,
            viewport        = 1 << 3,
            pipeline        = 1 << 4,
        };

        struct render_context_state {
            core::hash_t prev_rt_hash{ 0 };
            core::hash_t prev_vbuffer_hash{ 0 };
            core::hash_t prev_ibuffer_hash{ 0 };
            core::hash_t prev_viewport_hash{ 0 };
            pipeline_state_t prev_pipeline_id { no_pipeline_state };
            srb_t prev_srb{ no_srb };
        };

        struct renderer_state {
            io::ILog* log{ null };
            render_command_data default_cmd{};
            render_context_state context_state{};
            u32 dirty_flags { (u32)renderer_dirty_flags::none };
        };
        extern renderer_state g_renderer_state;

        void renderer__init();
        void renderer__deinit();
        void renderer__reset_state(bool reset_ctx_state = false);
        void renderer__set_render_targets();
        void renderer__set_vbuffers();
        void renderer__set_ibuffer();
        void renderer__set_viewport();
        void renderer__set_pipeline();
        void renderer__set_srb();
        void renderer__submit_render_state();
    }
}