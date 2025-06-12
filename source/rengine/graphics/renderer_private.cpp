#include "./renderer_private.h"
#include "./graphics_private.h"
#include "./render_target_manager_private.h"
#include "./pipeline_state_manager_private.h"
#include "./buffer_manager_private.h"
#include "./srb_manager_private.h"

#include "../exceptions.h"
#include "../strings.h"
#include "../core/hash.h"
#include "../math/math-types.h"

#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		renderer_state g_renderer_state = {};

		void renderer__init()
		{
			g_renderer_state.log = io::logger_use(strings::logs::g_renderer_tag);
		}

		void renderer__deinit()
		{
		}

		void renderer__reset_state(bool reset_ctx_state) {
			auto& cmd = g_renderer_state.default_cmd;
			auto viewport_size = g_graphics_state.viewport_size;
			cmd.name = strings::graphics::g_default_cmd_name;
			cmd.id = 0;
            cmd.hashes = {};
            cmd.viewport = { {0, 0}, viewport_size };
            cmd.scissor_rects.fill({});
            cmd.num_scissors = 0;
            cmd.topology = primitive_topology::triangle_list;
			cmd.depth_desc = {};
            cmd.blend_mode = blend_mode::replace;
            cmd.color_write = true;
            cmd.alpha_to_coverage = false;
            cmd.constant_depth_bias = 0.0f;
            cmd.slope_scaled_depth_bias = 0.0f;
            cmd.wireframe = false;
			cmd.num_vertex_buffers = 0;
			cmd.depth_stencil = no_render_target;
			cmd.render_targets.fill(no_render_target);
			cmd.render_targets[0] = cmd.depth_stencil = g_graphics_state.viewport_rt;
			cmd.num_render_targets = 1;
			cmd.vertex_buffers.fill(no_vertex_buffer);
			cmd.vertex_offsets.fill(0);
			cmd.index_buffer = no_index_buffer;
			cmd.index_offset = 0;
			cmd.pipeline_state = no_pipeline_state;

			if (reset_ctx_state)
				g_renderer_state.context_state = {};
		}

		void renderer__set_render_targets()
		{
			const auto ctx = g_graphics_state.contexts[0];
			const auto& cmd = g_renderer_state.default_cmd;
			auto& ctx_state = g_renderer_state.context_state;

			if (ctx_state.prev_rt_hash == cmd.hashes.render_targets)
				return;

			ctx_state.prev_rt_hash = cmd.hashes.render_targets;

			Diligent::ITextureView* render_targets[GRAPHICS_MAX_RENDER_TARGETS] = {};
			Diligent::ITextureView* depth_stencil = null;

			for (u8 i = 0; i < cmd.num_render_targets; ++i) {
				Diligent::ITexture* backbuffer = null;
				render_targets[i] = null;

				render_target_mgr__get_internal_handles(cmd.render_targets[i], &backbuffer, null);

				if (backbuffer)
					render_targets[i] = backbuffer->GetDefaultView(Diligent::TEXTURE_VIEW_RENDER_TARGET);
			}

			if (cmd.depth_stencil != no_render_target) {
				Diligent::ITexture* depthbuffer = null;
				render_target_mgr__get_internal_handles(cmd.depth_stencil,
					null,
					&depthbuffer);
				depth_stencil = depthbuffer->GetDefaultView(Diligent::TEXTURE_VIEW_DEPTH_STENCIL);
			}

			ctx->SetRenderTargets(cmd.num_render_targets,
				render_targets,
				depth_stencil,
				Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
		}

		void renderer__set_vbuffers()
		{
			const auto& cmd = g_renderer_state.default_cmd;
			const auto ctx = g_graphics_state.contexts[0];
			auto& ctx_state = g_renderer_state.context_state;

			const auto changed_offsets = ctx_state.prev_vbuffer_offsets_hash != cmd.hashes.vertex_buffer_offsets;
			const auto changed_buffers = ctx_state.prev_vbuffer_hash != cmd.hashes.vertex_buffers;
			const auto touched_buffers = changed_buffers || changed_offsets;

			if (!touched_buffers)
				return;

			Diligent::IBuffer* vertex_buffers[GRAPHICS_MAX_VBUFFERS] = {};
			for (u8 i = 0; i < cmd.num_vertex_buffers; ++i)
				buffer_mgr__get_handle(buffer_type::vertex_buffer, cmd.vertex_buffers[i], &vertex_buffers[i]);

			const auto flags = changed_buffers ? Diligent::SET_VERTEX_BUFFERS_FLAG_RESET : Diligent::SET_VERTEX_BUFFERS_FLAG_NONE;
			ctx->SetVertexBuffers(0,
				cmd.num_vertex_buffers,
				vertex_buffers,
				cmd.vertex_offsets.data(),
				Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION,
				flags);

			ctx_state.prev_vbuffer_hash = cmd.hashes.vertex_buffers;
			ctx_state.prev_vbuffer_offsets_hash = cmd.hashes.vertex_buffer_offsets;
		}

		void renderer__set_ibuffer()
		{
			const auto& cmd = g_renderer_state.default_cmd;
			const auto ctx = g_graphics_state.contexts[0];
			auto& ctx_state = g_renderer_state.context_state;

			if (ctx_state.prev_ibuffer_hash == cmd.index_buffer)
				return;

			Diligent::IBuffer* index_buffer;
			if (cmd.index_buffer == no_index_buffer)
				index_buffer = null;
			else
				buffer_mgr__get_handle(buffer_type::index_buffer, cmd.index_buffer, &index_buffer);

			ctx->SetIndexBuffer(index_buffer, cmd.index_offset, Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
			ctx_state.prev_ibuffer_hash = cmd.hashes.index_buffer;
		}

                void renderer__set_viewport()
                {
                        const auto& cmd = g_renderer_state.default_cmd;
                        const auto ctx = g_graphics_state.contexts[0];
                        const auto& viewport = cmd.viewport;
                        auto& ctx_state = g_renderer_state.context_state;

			if (ctx_state.prev_viewport_hash == cmd.hashes.viewport)
				return;

			auto rt_size = viewport.size;

			if (cmd.num_render_targets > 0)
				render_target_mgr__get_size(cmd.render_targets[0], &rt_size);

			Diligent::Viewport view;
			view.TopLeftX = viewport.position.x;
			view.TopLeftY = viewport.position.y;
			view.Width = viewport.size.x;
			view.Height = viewport.size.y;
			view.MinDepth = 0;
			view.MaxDepth = 1;
                        ctx->SetViewports(1, &view, rt_size.x, rt_size.y);
                        ctx_state.prev_viewport_hash = cmd.hashes.viewport;
                }

                void renderer__set_scissor_rects()
                {
                        const auto& cmd = g_renderer_state.default_cmd;
                        const auto ctx = g_graphics_state.contexts[0];
                        auto& ctx_state = g_renderer_state.context_state;

                        if (ctx_state.prev_scissor_hash == cmd.hashes.scissors)
                                return;

                        Diligent::Rect rects[GRAPHICS_MAX_SCISSORS] = {};
                        for (u8 i = 0; i < cmd.num_scissors; ++i) {
                                const auto& r = cmd.scissor_rects[i];
                                rects[i].left = (int)r.position.x;
                                rects[i].top = (int)r.position.y;
                                rects[i].right = (int)(r.position.x + r.size.x);
                                rects[i].bottom = (int)(r.position.y + r.size.y);
                        }

                        auto rt_size = cmd.viewport.size;
                        if (cmd.num_render_targets > 0)
                                render_target_mgr__get_size(cmd.render_targets[0], &rt_size);

                        ctx->SetScissorRects(cmd.num_scissors, rects, rt_size.x, rt_size.y);
                        ctx_state.prev_scissor_hash = cmd.hashes.scissors;
                }

		void renderer__set_pipeline()
		{
			const auto& cmd = g_renderer_state.default_cmd;
			const auto ctx = g_graphics_state.contexts[0];
			auto& ctx_state = g_renderer_state.context_state;

			if (ctx_state.prev_pipeline_id == cmd.pipeline_state)
				return;

			Diligent::IPipelineState* pipeline = null;
			pipeline_state_mgr__get_internal_handle(cmd.pipeline_state, &pipeline);

			ctx->SetPipelineState(pipeline);
			ctx_state.prev_pipeline_id = cmd.pipeline_state;
		}

		void renderer__set_srb()
		{
			const auto& cmd = g_renderer_state.default_cmd;
			const auto ctx = g_graphics_state.contexts[0];
			auto& ctx_state = g_renderer_state.context_state;

			if (ctx_state.prev_srb == cmd.srb)
				return;

			Diligent::IShaderResourceBinding* srb = null;
			srb_mgr__get_handle(cmd.srb, &srb);

			ctx->CommitShaderResources(srb, Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
			ctx_state.prev_srb = cmd.srb;
		}

		void renderer__submit_render_state()
		{
			renderer__set_render_targets();
                        renderer__set_vbuffers();
                        renderer__set_ibuffer();
                        renderer__set_viewport();
                        renderer__set_scissor_rects();
                        renderer__set_pipeline();
                        renderer__set_srb();
                }
	}
}