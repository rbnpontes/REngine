#include "./renderer.h"
#include "./renderer_private.h"
#include "./graphics_private.h"
#include "./render_target_manager_private.h"
#include "./render_command_private.h"

#include "../core/allocator.h"
#include "../core/window_private.h"
#include "../core/profiler.h"
#include "../core/string_pool.h"
#include "../io/io.h"
#include "../exceptions.h"

#include <SwapChain.h>
#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		void renderer_clear(const clear_desc& desc) {
			profile();
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
			profile();
			renderer__reset_state();
		}

		void renderer_use_command(const render_command_t& command)
		{
			profile();
			const auto log = g_renderer_state.log;
			auto& state = g_renderer_state;

			// if same command has been set, we must skip
			if (state.default_cmd.id == command)
				return;

			if (!render_command__get(command, &state.default_cmd))
				return;

			auto& cmd = state.default_cmd;
			if (cmd.hashes.render_targets != state.context_state.prev_rt_hash) {
				state.dirty_flags |= (u32)renderer_dirty_flags::render_targets;
				state.context_state.prev_rt_hash = cmd.hashes.render_targets;
			}

			if (cmd.hashes.vertex_buffers != state.context_state.prev_vbuffer_hash)
				state.dirty_flags |= (u32)renderer_dirty_flags::vertex_buffer;

			if (cmd.hashes.index_buffer != state.context_state.prev_ibuffer_hash)
				state.dirty_flags |= (u32)renderer_dirty_flags::index_buffer;

			if (cmd.hashes.viewport != state.context_state.prev_viewport_hash)
				state.dirty_flags |= (u32)renderer_dirty_flags::viewport;

			if (cmd.pipeline_state != state.context_state.prev_pipeline_id)
				state.dirty_flags |= (u32)renderer_dirty_flags::pipeline;

			renderer__submit_render_state();
		}

		void renderer_set_vbuffer(const vertex_buffer_t& buffer, u64 offset)
		{
			return renderer_set_vbuffers(&buffer, 1, &offset);
		}

		void renderer_set_vbuffers(const vertex_buffer_t* buffers, u8 num_buffers, u64* offsets)
		{
			auto& cmd = g_renderer_state.default_cmd;
			render_command__set_vbuffers(cmd, buffers, num_buffers, offsets);
		}

		void renderer_set_ibuffer(const index_buffer_t& buffer, u64 offset)
		{
			auto& cmd = g_renderer_state.default_cmd;
			render_command__set_ibuffer(cmd, buffer, offset);
		}

		void renderer_set_render_target(const render_target_t& rt_id, const render_target_t& depth_id)
		{
			renderer_set_render_targets(&rt_id, 1, depth_id);
		}

		void renderer_set_render_targets(const render_target_t* render_targets, const u8& num_rts, const render_target_t& depth_id)
		{
			auto& cmd = g_renderer_state.default_cmd;
			render_command__set_rts(cmd, render_targets, num_rts, depth_id);
		}

		void renderer_set_texture_2d(c_str slot_name, const texture_2d_t& tex_id)
		{
			auto& cmd = g_renderer_state.default_cmd;
			core::hash_t slot_hash{};
			core::string_pool_intern(slot_name, &slot_hash);
			render_command__set_tex2d(cmd, slot_hash, tex_id);
		}

		void renderer_set_texture_3d(c_str slot_name, const texture_3d_t& tex_id)
		{
			auto& cmd = g_renderer_state.default_cmd;
			core::hash_t slot_hash{};
			core::string_pool_intern(slot_name, &slot_hash);
			render_command__set_tex3d(cmd, slot_hash, tex_id);
		}

		void renderer_set_texture_cube(c_str slot_name, const texture_cube_t& tex_id)
		{
			auto& cmd = g_renderer_state.default_cmd;
			core::hash_t slot_hash{};
			core::string_pool_intern(slot_name, &slot_hash);
			render_command__set_texcube(cmd, slot_hash, tex_id);
		}

		void renderer_set_texture_array(c_str slot_name, const texture_array_t& tex_id)
		{
			auto& cmd = g_renderer_state.default_cmd;
			core::hash_t slot_hash{};
			core::string_pool_intern(slot_name, &slot_hash);
			render_command__set_texarray(cmd, slot_hash, tex_id);
		}

		void renderer_set_texture_2d(core::hash_t slot, const texture_2d_t& tex_id)
		{
			auto& cmd = g_renderer_state.default_cmd;
			render_command__set_tex2d(cmd, slot, tex_id);
		}

		void renderer_set_texture_3d(core::hash_t slot, const texture_3d_t& tex_id)
		{
			auto& cmd = g_renderer_state.default_cmd;
			render_command__set_tex3d(cmd, slot, tex_id);
		}

		void renderer_set_texture_cube(core::hash_t slot, const texture_cube_t& tex_id)
		{
			auto& cmd = g_renderer_state.default_cmd;
			render_command__set_texcube(cmd, slot, tex_id);
		}

		void renderer_set_texture_array(core::hash_t slot, const texture_array_t& tex_id)
		{
			auto& cmd = g_renderer_state.default_cmd;
			render_command__set_texarray(cmd, slot, tex_id);
		}

		void renderer_set_viewport(const math::urect& rect)
		{
			auto& cmd = g_renderer_state.default_cmd;
			render_command__set_viewport(cmd, rect);
		}

		void renderer_set_topology(const primitive_topology& topology)
		{
			auto& cmd = g_renderer_state.default_cmd;
			render_command__set_topology(cmd, topology);
		}

		void renderer_set_cull_mode(const cull_mode& cull)
		{
			auto& cmd = g_renderer_state.default_cmd;
			render_command__set_cull(cmd, cull);
		}

                void renderer_set_program(const shader_program_t& program_id)
                {
                        auto& cmd = g_renderer_state.default_cmd;
                        render_command__set_program(cmd, program_id);
                }

                void renderer_set_depth(const depth_desc& desc)
                {
                        auto& cmd = g_renderer_state.default_cmd;
                        render_command__set_depth(cmd, desc);
                }

                void renderer_set_blend_mode(const blend_mode& mode)
                {
                        auto& cmd = g_renderer_state.default_cmd;
                        render_command__set_blend_mode(cmd, mode);
                }

                void renderer_set_color_write(const bool enabled)
                {
                        auto& cmd = g_renderer_state.default_cmd;
                        render_command__set_color_write(cmd, enabled);
                }

                void renderer_set_alpha_to_coverage(const bool enabled)
                {
                        auto& cmd = g_renderer_state.default_cmd;
                        render_command__set_alpha_to_coverage(cmd, enabled);
                }

                void renderer_set_scissors(const bool enabled)
                {
                        auto& cmd = g_renderer_state.default_cmd;
                        render_command__set_scissors(cmd, enabled);
                }

                void renderer_set_constant_depth_bias(float bias)
                {
                        auto& cmd = g_renderer_state.default_cmd;
                        render_command__set_constant_depth_bias(cmd, bias);
                }

                void renderer_set_slope_scaled_depth_bias(float bias)
                {
                        auto& cmd = g_renderer_state.default_cmd;
                        render_command__set_slope_scaled_depth_bias(cmd, bias);
                }

		void renderer_set_wireframe(const bool enabled)
		{
			auto& cmd = g_renderer_state.default_cmd;
			render_command__set_wireframe(cmd, enabled);
		}

		void renderer_set_material(const material_t& material_id)
		{
			throw not_implemented_exception();
		}

		void renderer_flush()
		{
			profile();
			auto& cmd = g_renderer_state.default_cmd;
			auto prev_cmd_hash = cmd.id;

			render_command__prepare(cmd);

			if (cmd.id == prev_cmd_hash && cmd.pipeline_state != no_pipeline_state)
				return;

			render_command__build_internal_objects(cmd);
			renderer__submit_render_state();
		}

		void renderer_draw(const draw_desc& desc) {
			profile();
			using namespace Diligent;
			renderer_flush();

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

		void renderer_draw_indexed(const draw_indexed_desc& desc)
		{
			profile();
			using namespace Diligent;
			renderer_flush();

			const auto ctx = g_graphics_state.contexts[0];
			DrawIndexedAttribs draw_attr;
#if ENGINE_DEBUG
			draw_attr.Flags = DRAW_FLAG_VERIFY_ALL;
#else
			draw_attr.Flags = DRAW_FLAG_NONE;
#endif
			draw_attr.NumIndices = desc.num_indices;
			draw_attr.IndexType = desc.use_32bit_indices ? VT_UINT32 : VT_UINT16;
			draw_attr.NumInstances = desc.num_instances;
			draw_attr.FirstIndexLocation = desc.start_index_idx;
			draw_attr.BaseVertex = desc.start_vertex_idx;
			draw_attr.FirstInstanceLocation = desc.start_instance_idx;

			ctx->DrawIndexed(draw_attr);
		}

		void renderer_blit(const render_target_t& src, const render_target_t& dst)
		{
			throw not_implemented_exception();
		}
	}
}