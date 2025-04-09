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
            viewport        = 1 << 4,
            build_pipeline  = 1 << 5,
            pipeline        = 1 << 6,
        };

        struct render_command_hashes {
            core::hash_t render_targets{ 0 };
            core::hash_t vertex_buffers{ 0 };
            core::hash_t viewport{ 0 };
            core::hash_t graphics_state{ 0 };
        };

        struct render_context_state {
            core::hash_t prev_rt_hash{ 0 };
            core::hash_t prev_vbuffer_hash{ 0 };
            core::hash_t prev_ibuffer_hash{ 0 };
            core::hash_t prev_viewport_hash{ 0 };
            pipeline_state_t prev_pipeline_id { no_pipeline_state };
        };

        struct render_command_data {
            u32 id{0};
            string name { strings::graphics::g_default_cmd_name };

            array<render_target_t, GRAPHICS_MAX_RENDER_TARGETS> render_targets{};
            array<vertex_buffer_t, GRAPHICS_MAX_VBUFFERS> vertex_buffers{};
            array<u64, GRAPHICS_MAX_VBUFFERS> vertex_offsets{};
            index_buffer_t index_buffer{};
            render_target_t depth_stencil{no_render_target};
            u8 num_render_targets{ 0 };
            u8 num_vertex_buffers{ 0 };
            primitive_topology topology{ primitive_topology::triangle_list };
            cull_mode cull{ cull_mode::clock_wise };
            bool wireframe{ false };
            bool depth_enabled{ true };
            u32 vertex_elements{ 0 };
            math::urect viewport{};
			shader_t vertex_shader { no_shader };
			shader_t pixel_shader { no_shader };

			pipeline_state_t pipeline_state { no_pipeline_state };
            render_command_hashes hashes{};
        };

        typedef hash_map<render_command_t, shared_ptr<render_command_data>> command_list;
        struct renderer_state {
            io::ILog* log{ null };
            render_command_data default_cmd{};
            render_context_state context_state{};
            u32 dirty_flags { (u32)renderer_dirty_flags::none };
            command_list commands;
            u32 num_commands;
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
        void renderer__submit_render_state();
        void renderer__assert_render_target_idx(u8 idx);
        void renderer__build_graphics_pipeline();
        void renderer__build_command_hashes();
    }
}