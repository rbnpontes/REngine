#include "./renderer.h"
#include "./renderer_private.h"
#include "./graphics_private.h"
#include "./render_target_manager_private.h"

#include "../core/window_private.h"
#include "../io/io.h"
#include "../exceptions.h"

#include <SwapChain.h>
#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		void renderer_clear(const clear_desc& desc) {
			const auto& state = g_renderer_state;
			const auto ctx = g_graphics_state.contexts[0];
			const auto log = state.log;

			renderer__set_render_targets();
			renderer__set_viewport();

			if (g_renderer_state.num_render_targets > 0) {
				Diligent::ITexture* rt = null;
				render_target_mgr__get_internal_handles(g_renderer_state.render_targets[0], &rt, null);

				ctx->ClearRenderTarget(rt->GetDefaultView(Diligent::TEXTURE_VIEW_RENDER_TARGET),
					desc.color,
					Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
			}

			if (!(desc.clear_depth || desc.clear_stencil))
				return;

			if (!g_renderer_state.depth_stencil == no_render_target) {
				log->warn(strings::logs::g_renderer_cant_clear_unset_depthbuffer);
				return;
			}

			Diligent::CLEAR_DEPTH_STENCIL_FLAGS flags;
			if (desc.clear_depth)
				flags |= Diligent::CLEAR_DEPTH_FLAG;
			if (desc.clear_stencil)
				flags |= Diligent::CLEAR_STENCIL_FLAG;

			Diligent::ITexture* depthbuffer = null;
			render_target_mgr__get_internal_handles(g_renderer_state.depth_stencil,
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
			throw not_implemented_exception();
		}

		void renderer_set_vbuffer(const vertex_buffer_t& buffer, u64 offset)
		{
			throw not_implemented_exception();
		}

		void renderer_set_vbuffers(const vertex_buffer_t* buffers, u8 num_buffers, u64* offsets)
		{
			throw not_implemented_exception();
		}

		void renderer_set_ibuffer(const index_buffer_t& buffer)
		{
			throw not_implemented_exception();
		}

		void renderer_set_render_target(const render_target_t& rt_id)
		{
			renderer_set_render_targets(&rt_id, 1);
		}

		void renderer_set_render_targets(const render_target_t* render_targets, const u8& num_rts)
		{
			const auto log = g_renderer_state.log;

			bool dirty = false;
			u8 count = num_rts;
			if (num_rts >= GRAPHICS_MAX_RENDER_TARGETS) {
				count = GRAPHICS_MAX_RENDER_TARGETS;

				log->warn(fmt::format(strings::logs::g_renderer_isnt_allowed_to_set_rt_grt_than_max,
					num_rts,
					GRAPHICS_MAX_RENDER_TARGETS).c_str()
				);
			}
			
			for (u8 i = 0; i < count; ++i) {
				dirty |= g_renderer_state.render_targets[i] != render_targets[i];
				g_renderer_state.render_targets[i] = render_targets[i];
			}

			if (dirty)
				g_renderer_state.dirty_flags |= (u32)renderer_dirty_flags::render_targets;
			g_renderer_state.num_render_targets = count;
		}

		void renderer_set_depthbuffer(const render_target_t& rt_id)
		{
			if (g_renderer_state.depth_stencil != rt_id)
				g_renderer_state.dirty_flags |= (u32)renderer_dirty_flags::depth_stencil;
			g_renderer_state.depth_stencil = rt_id;
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

		void renderer_set_pipeline(const pipeline_state_t& pipeline_id)
		{
			throw not_implemented_exception();
		}

		void renderer_set_viewport(const math::urect& rect)
		{
			if (!rect.equal(g_renderer_state.viewport))
				g_renderer_state.dirty_flags |= (u32)renderer_dirty_flags::viewport;
			g_renderer_state.viewport = rect;
			
		}

		void renderer_set_topology(const primitive_topology& topology)
		{
			throw not_implemented_exception();
		}

		void renderer_set_cull_mode(const cull_mode& cull)
		{
			throw not_implemented_exception();
		}

		void renderer_set_vertex_elements(const u32& vertex_elements)
		{
			throw not_implemented_exception();
		}

		void renderer_set_vertex_shader(const shader_t& shader_id)
		{
			throw not_implemented_exception();
		}

		void renderer_set_pixel_shader(const shader_t& shader_id)
		{
			throw not_implemented_exception();
		}

		void renderer_set_depth_enabled(const bool& enabled)
		{
			throw not_implemented_exception();
		}

		void renderer_set_dbg_name(c_str dbg_name)
		{
			throw not_implemented_exception();
		}

		void renderer_set_material(const material_t& material_id)
		{
			throw not_implemented_exception();
		}

		void renderer_flush()
		{
			renderer__submit_render_state();
		}

		void renderer_draw() {
			renderer__submit_render_state();

			//const auto ctx = get_state()->contexts;
		}
		
		void renderer_blit(const render_target_t& src, const render_target_t& dst)
		{
			throw not_implemented_exception();
		}
	}
}