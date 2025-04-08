#include "./renderer.h"
#include "./renderer_private.h"
#include "./graphics_private.h"
#include "./render_target_manager_private.h"

#include "../core/allocator.h"
#include "../core/window_private.h"
#include "../io/io.h"
#include "../exceptions.h"

#include <SwapChain.h>
#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		void renderer_clear(const clear_desc& desc) {
			const auto& cmd = g_renderer_state.default_cmd;
			const auto ctx = g_graphics_state.contexts[0];
			const auto log = g_renderer_state.log;

			renderer__set_render_targets();
			renderer__set_viewport();

			if (cmd.num_render_targets > 0) {
				Diligent::ITexture* rt = null;
				render_target_mgr__get_internal_handles(cmd.render_targets[0], &rt, null);

				ctx->ClearRenderTarget(rt->GetDefaultView(Diligent::TEXTURE_VIEW_RENDER_TARGET),
					desc.color,
					Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
			}

			if (!(desc.clear_depth || desc.clear_stencil))
				return;

			if (!cmd.depth_stencil == no_render_target) {
				log->warn(strings::logs::g_renderer_cant_clear_unset_depthbuffer);
				return;
			}

			Diligent::CLEAR_DEPTH_STENCIL_FLAGS flags;
			if (desc.clear_depth)
				flags |= Diligent::CLEAR_DEPTH_FLAG;
			if (desc.clear_stencil)
				flags |= Diligent::CLEAR_STENCIL_FLAG;

			Diligent::ITexture* depthbuffer = null;
			render_target_mgr__get_internal_handles(cmd.depth_stencil,
				null,
				&depthbuffer);
			ctx->ClearDepthStencil(depthbuffer->GetDefaultView(Diligent::TEXTURE_VIEW_DEPTH_STENCIL),
				flags,
				desc.depth,
				desc.stencil,
				Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
		}

		void renderer_reset_states()
		{
			renderer__reset_state();
		}

		void renderer_use_command(const render_command_t& command)
		{
			const auto log = g_renderer_state.log;
			auto& state = g_renderer_state;
			const auto& it = state.commands.find_as(command);
			if (it == state.commands.end()) {
				log->warn(
					fmt::format(strings::logs::g_renderer_not_found_command, command).c_str()
				);
				return;
			}

			auto cmd = it->second;
			auto& curr_cmd = state.default_cmd;

			// if same command has been set, we must skip
			if (cmd->id == curr_cmd.id)
				return;

			if (cmd->hashes.render_targets != curr_cmd.hashes.render_targets) {
				state.dirty_flags |= (u32)renderer_dirty_flags::render_targets;
				if(cmd->depth_stencil != curr_cmd.depth_stencil)
					state.dirty_flags |= (u32)renderer_dirty_flags::depth_stencil;
			}

			if (cmd->hashes.vertex_buffers != curr_cmd.hashes.vertex_buffers) {
				state.dirty_flags |= (u32)renderer_dirty_flags::vertex_buffer;

				if (cmd->index_buffer != curr_cmd.index_buffer)
					state.dirty_flags |= (u32)renderer_dirty_flags::index_buffer;
			}

			if (cmd->hashes.graphics_state != curr_cmd.hashes.graphics_state) {
				if (!cmd->viewport.equal(curr_cmd.viewport))
					state.dirty_flags |= (u32)renderer_dirty_flags::viewport;
			}

			if (cmd->pipeline_state != curr_cmd.pipeline_state)
				state.dirty_flags |= (u32)renderer_dirty_flags::pipeline;

			// remove any build pipeline flag. we already have 
			// a pipeline on command, so this must be skip
			state.dirty_flags ^= (u32)renderer_dirty_flags::build_pipeline;
			state.default_cmd = *cmd;
		}

		render_command_t renderer_build_command(c_str cmd_name)
		{
			auto& curr_cmd = g_renderer_state.default_cmd;
			if (!cmd_name)
				cmd_name = strings::graphics::g_default_cmd_name;

			renderer__build_command_hashes();

			const auto& it = g_renderer_state.commands.find_as(curr_cmd.id);
			if (it != g_renderer_state.commands.end())
				return curr_cmd.id;

			if (g_renderer_state.num_commands == GRAPHICS_MAX_RENDER_COMMANDS)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_renderer_cant_build_render_cmd, GRAPHICS_MAX_RENDER_COMMANDS).c_str()
				);

			auto cmd = shared_ptr<render_command_data>(core::alloc_new<render_command_data>());
			// lets build our pipeline
			g_renderer_state.default_cmd.name = cmd_name;
			renderer_flush();
			// now copy default command to new command
			*cmd = g_renderer_state.default_cmd;
			g_renderer_state.default_cmd.name = strings::graphics::g_default_cmd_name;
			// insert command into command list
			g_renderer_state.commands[curr_cmd.id] = cmd;
			++g_renderer_state.num_commands;
			return curr_cmd.id;
		}

		void renderer_destroy_command(const render_command_t& command)
		{
			const auto& it = g_renderer_state.commands.find_as(command);
			if (it == g_renderer_state.commands.end())
				return;
			g_renderer_state.commands.erase(it);
			--g_renderer_state.num_commands;
		}

		void renderer_set_vbuffer(const vertex_buffer_t& buffer, u64 offset)
		{
			return renderer_set_vbuffers(&buffer, 1, &offset);
		}

		void renderer_set_vbuffers(const vertex_buffer_t* buffers, u8 num_buffers, u64* offsets)
		{
			const auto log = g_renderer_state.log;
			auto& cmd = g_renderer_state.default_cmd;

			u8 count = num_buffers;
			if (num_buffers >= GRAPHICS_MAX_VBUFFERS) {
				count = GRAPHICS_MAX_VBUFFERS;

				log->warn(fmt::format(strings::logs::g_renderer_isnt_allowed_to_set_buffer_grt_than_max,
					num_buffers,
					GRAPHICS_MAX_VBUFFERS).c_str()
				);
			}

			core::hash_t hash = count;
			for (u8 i = 0; i < count; ++i) {
				hash = core::hash_combine(hash, buffers[i]);
				hash = core::hash_combine(hash, offsets[i]);

				cmd.vertex_buffers[i] = buffers[i];
				cmd.vertex_offsets[i] = offsets[i];
			}

			cmd.num_vertex_buffers = count;
			cmd.hashes.vertex_buffers = hash;
		}

		void renderer_set_ibuffer(const index_buffer_t& buffer)
		{
			auto& cmd = g_renderer_state.default_cmd;
			cmd.index_buffer = buffer;
		}

		void renderer_set_render_target(const render_target_t& rt_id, const render_target_t& depth_id)
		{
			renderer_set_render_targets(&rt_id, 1, depth_id);
		}

		void renderer_set_render_targets(const render_target_t* render_targets, const u8& num_rts, const render_target_t& depth_id)
		{
			const auto log = g_renderer_state.log;

			u8 count = num_rts;
			if (num_rts >= GRAPHICS_MAX_RENDER_TARGETS) {
				count = GRAPHICS_MAX_RENDER_TARGETS;

				log->warn(fmt::format(strings::logs::g_renderer_isnt_allowed_to_set_rt_grt_than_max,
					num_rts,
					GRAPHICS_MAX_RENDER_TARGETS).c_str()
				);
			}
			
			auto& cmd = g_renderer_state.default_cmd;
			core::hash_t hash = count;
			for (u8 i = 0; i < count; ++i) {
				hash = core::hash_combine(hash, render_targets[i]);
				if (cmd.render_targets[i] != render_targets[i])
					cmd.pipeline_state = no_pipeline_state;
				cmd.render_targets[i] = render_targets[i];
			}
			cmd.hashes.render_targets = core::hash_combine(hash, depth_id);

			if(cmd.num_render_targets != count)
				cmd.pipeline_state = no_pipeline_state;

			cmd.num_render_targets = count;
			cmd.depth_stencil = depth_id;
		}

		void renderer_set_texture_2d(const u8& tex_slot, const texture_2d_t& tex_id)
		{
			throw not_implemented_exception();
		}

		void renderer_set_texture_3d(const u8& tex_slot, const texture_3d_t& tex_id)
		{
			throw not_implemented_exception();
		}

		void renderer_set_texture_cube(const u8& tex_slot, const texture_cube_t& tex_id)
		{
			throw not_implemented_exception();
		}

		void renderer_set_texture_array(const u8& tex_slot, const texture_array_t& tex_id)
		{
			throw not_implemented_exception();
		}

		void renderer_set_viewport(const math::urect& rect)
		{
			auto& cmd = g_renderer_state.default_cmd;
			cmd.viewport = rect;
			cmd.hashes.viewport = rect.to_hash();
		}

		void renderer_set_topology(const primitive_topology& topology)
		{
			auto& cmd = g_renderer_state.default_cmd;
			if (cmd.topology != topology)
				cmd.pipeline_state = no_pipeline_state;
			cmd.topology = topology;
		}

		void renderer_set_cull_mode(const cull_mode& cull)
		{
			auto& cmd = g_renderer_state.default_cmd;
			if (cmd.cull != cull)
				cmd.pipeline_state = no_pipeline_state;
			cmd.cull = cull;
		}

		void renderer_set_vertex_elements(const u32& vertex_elements)
		{
			auto& cmd = g_renderer_state.default_cmd;
			if (cmd.vertex_elements != vertex_elements)
				cmd.pipeline_state = no_pipeline_state;
			cmd.vertex_elements = vertex_elements;
		}

		void renderer_set_vertex_shader(const shader_t& shader_id)
		{
			auto& cmd = g_renderer_state.default_cmd;
			if (cmd.vertex_shader != shader_id)
				cmd.pipeline_state = no_pipeline_state;
			cmd.vertex_shader = shader_id;
		}

		void renderer_set_pixel_shader(const shader_t& shader_id)
		{
			auto& cmd = g_renderer_state.default_cmd;
			if (cmd.pixel_shader != shader_id)
				cmd.pipeline_state = no_pipeline_state;
			cmd.pixel_shader = shader_id;
		}

		void renderer_set_depth_enabled(const bool& enabled)
		{
			auto& cmd = g_renderer_state.default_cmd;
			if (cmd.depth_enabled != enabled)
				cmd.pipeline_state = no_pipeline_state;
			cmd.depth_enabled = enabled;
		}

		void renderer_set_wireframe(const bool enabled)
		{
			auto& cmd = g_renderer_state.default_cmd;
			if (cmd.wireframe != enabled)
				cmd.pipeline_state = no_pipeline_state;
			cmd.wireframe = enabled;
		}

		void renderer_set_material(const material_t& material_id)
		{
			throw not_implemented_exception();
		}

		void renderer_flush()
		{
			renderer__build_graphics_pipeline();
			renderer__submit_render_state();
		}

		void renderer_draw(const draw_desc& desc) {
			using namespace Diligent;
			if (g_renderer_state.default_cmd.pipeline_state == no_pipeline_state)
				renderer__build_graphics_pipeline();
			renderer__submit_render_state();

			const auto ctx = g_graphics_state.contexts[0];
			DrawAttribs draw_attr;
#if ENGINE_DEBUG
			draw_attr.Flags = DRAW_FLAG_VERIFY_ALL;
#else
			draw_attr.Flags = DRAW_FLAG_NONE;
#endif
			draw_attr.FirstInstanceLocation = desc.start_instance_idx;
			draw_attr.StartVertexLocation = desc.start_vertex_idx;
			draw_attr.NumInstances = desc.num_instances;
			draw_attr.NumVertices = desc.num_vertices;

			ctx->Draw(draw_attr);
		}
		
		void renderer_blit(const render_target_t& src, const render_target_t& dst)
		{
			throw not_implemented_exception();
		}
	}
}