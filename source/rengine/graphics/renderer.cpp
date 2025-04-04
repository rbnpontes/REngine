#include "./renderer.h"
#include "./renderer_private.h"
#include "./graphics_private.h"

#include "../core/window_private.h"
#include "../io/io.h"
#include "../exceptions.h"

#include <SwapChain.h>

namespace rengine {
	namespace graphics {
		void renderer_set_window(core::window_t window_id) {
			const auto& data = core::window__get_data(window_id);
			const auto swap_chain = (Diligent::ISwapChain*)data.swap_chain;

			if (!swap_chain)
				return;

			g_renderer_state.render_targets[0] = swap_chain->GetCurrentBackBufferRTV();
			g_renderer_state.depth_stencil = swap_chain->GetDepthBufferDSV();
			g_renderer_state.num_render_targets = 1;
			g_renderer_state.dirty_flags |= (u32)renderer_dirty_flags::render_targets;
		}

		void renderer_clear(const clear_desc& desc)
		{
			if ((g_renderer_state.dirty_flags & (u32)renderer_dirty_flags::render_targets) != 0)
				renderer__set_render_targets();

			const auto ctx = g_graphics_state.contexts[0];

			if (g_renderer_state.num_render_targets > 0) {
				renderer__assert_render_target_idx(desc.render_target_index);
				ctx->ClearRenderTarget(g_renderer_state.render_targets[desc.render_target_index],
					desc.color,
					Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
			}

			if (!(desc.clear_depth || desc.clear_stencil))
				return;

			if (!g_renderer_state.depth_stencil)
				throw graphics_exception(strings::exceptions::g_renderer_clear_depth_without_set);

			Diligent::CLEAR_DEPTH_STENCIL_FLAGS flags;
			if (desc.clear_depth)
				flags |= Diligent::CLEAR_DEPTH_FLAG;
			if (desc.clear_stencil)
				flags |= Diligent::CLEAR_STENCIL_FLAG;

			ctx->ClearDepthStencil(g_renderer_state.depth_stencil,
				flags,
				desc.depth,
				desc.stencil,
				Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
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

		void renderer_set_render_target(const render_target_t& rt_id, const render_target_t& depth_stencil)
		{
			throw not_implemented_exception();
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
	}
}