#include "./renderer_private.h"
#include "./graphics_private.h"
#include "./render_target_manager_private.h"
#include "./pipeline_state_manager_private.h"
#include "./buffer_manager_private.h"

#include "../exceptions.h"
#include "../strings.h"
#include "../core/hash.h"

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
			cmd.name = strings::graphics::g_default_cmd_name;
			cmd.id = 0;
			cmd.hashes = {};
			cmd.viewport = math::urect::zero;
			cmd.topology = primitive_topology::triangle_list;
			cmd.depth_enabled = true;
			cmd.wireframe = false;
			cmd.num_render_targets = cmd.num_vertex_buffers = 0;
			cmd.depth_stencil = no_render_target;
			cmd.render_targets.fill(no_render_target);
			cmd.vertex_buffers.fill(no_vertex_buffer);
			cmd.vertex_offsets.fill(0);
			cmd.index_buffer = no_index_buffer;
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

			if (ctx_state.prev_vbuffer_hash == cmd.hashes.vertex_buffers)
				return;

			Diligent::IBuffer* vertex_buffers[GRAPHICS_MAX_VBUFFERS] = {};
			for (u8 i = 0; i < cmd.num_vertex_buffers; ++i)
				buffer_mgr__get_handle(buffer_type::vertex_buffer, cmd.vertex_buffers[i], &vertex_buffers[i]);

			ctx->SetVertexBuffers(0,
				cmd.num_vertex_buffers,
				vertex_buffers,
				cmd.vertex_offsets.data(),
				Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION,
				Diligent::SET_VERTEX_BUFFERS_FLAG_NONE);

			ctx_state.prev_vbuffer_hash = cmd.hashes.vertex_buffers;
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

			// TODO: expose index buffer offset on cmd
			ctx->SetIndexBuffer(index_buffer, 0, Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
			ctx_state.prev_ibuffer_hash = cmd.index_buffer;
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

		void renderer__set_pipeline()
		{
			const auto& cmd = g_renderer_state.default_cmd;
			const auto ctx = g_graphics_state.contexts[0];
			auto& ctx_state = g_renderer_state.context_state;

			if (ctx_state.prev_pipeline_id == cmd.pipeline_state)
				return;

			Diligent::IPipelineState* pipeline;
			pipeline_state_mgr__get_internal_handle(cmd.pipeline_state, &pipeline);

			ctx->SetPipelineState(pipeline);
			ctx_state.prev_pipeline_id = cmd.pipeline_state;
		}

		void renderer__submit_render_state()
		{
			renderer__set_render_targets();
			renderer__set_vbuffers();
			renderer__set_ibuffer();
			renderer__set_viewport();
			renderer__set_pipeline();
		}

		void renderer__build_graphics_pipeline()
		{
			auto& cmd = g_renderer_state.default_cmd;
			if (cmd.pipeline_state != no_pipeline_state)
				return;

			const auto name = fmt::format("{0}::gpipeline", cmd.name);
			// lets build our pipeline
			graphics_pipeline_state_create pipeline_create;
			pipeline_create.name = name.c_str();
			pipeline_create.cull = cmd.cull;
			pipeline_create.depth = cmd.depth_enabled;
			pipeline_create.wireframe = cmd.wireframe;
			pipeline_create.topology = cmd.topology;
			pipeline_create.vertex_shader = cmd.vertex_shader;
			pipeline_create.pixel_shader = cmd.pixel_shader;
			// TODO: implement scissors
			// pipeline_create.scissors = cmd.scissors;
			pipeline_create.num_render_targets = cmd.num_render_targets;

			render_target_desc rt_desc;
			for (u8 i = 0; i < cmd.num_render_targets; ++i) {
				render_target_mgr__get_desc(cmd.render_targets[i], &rt_desc);
				pipeline_create.render_target_formats[i] = rt_desc.format;
			}

			if (cmd.depth_stencil != no_render_target) {
				render_target_mgr__get_desc(cmd.depth_stencil, &rt_desc);
				pipeline_create.depth_stencil_format = rt_desc.depth_format;
			}

			pipeline_create.vertex_elements = cmd.vertex_elements;
			cmd.pipeline_state = pipeline_state_mgr_create_graphics(pipeline_create);
		}

		void renderer__build_command_hashes()
		{
			auto& cmd = g_renderer_state.default_cmd;
			auto& hashes = cmd.hashes;
			// calculate graphics state hashes
			hashes.graphics_state = cmd.vertex_elements;
			hashes.graphics_state = core::hash_combine(hashes.graphics_state, (u32)cmd.topology);
			hashes.graphics_state = core::hash_combine(hashes.graphics_state, (u32)cmd.cull);
			hashes.graphics_state = core::hash_combine(hashes.graphics_state, (u32)cmd.wireframe);
			hashes.graphics_state = core::hash_combine(hashes.graphics_state, (u32)cmd.depth_enabled);

			cmd.id = core::hash_combine(hashes.render_targets, hashes.vertex_buffers);
			cmd.id = core::hash_combine(cmd.id, cmd.index_buffer);
			cmd.id = core::hash_combine(cmd.id, hashes.viewport);
			cmd.id = core::hash_combine(cmd.id, hashes.graphics_state);
		}
	}
}