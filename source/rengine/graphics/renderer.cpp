#include "./renderer.h"
#include "./renderer_private.h"
#include "./graphics_private.h"
#include "../core/window_private.h"
#include <SwapChain.h>

namespace rengine {
	namespace graphics {
		void renderer_set_window(core::window_t window_id) {
			const auto& data = core::window__get_data(window_id);
			const auto swap_chain = (Diligent::ISwapChain*)data.swap_chain;

			if (!swap_chain)
				return;

			g_renderer_state.render_target[0] = swap_chain->GetCurrentBackBufferRTV();
			g_renderer_state.depth_stencil = swap_chain->GetDepthBufferDSV();
			g_renderer_state.num_render_targets = 1;
			g_renderer_state.dirty_flags |= (u32)renderer_dirty_flags::render_targets;
		}

		void renderer_set_clear_color(const clear_color_desc& desc)
		{
			if (desc.render_target_index >= DILIGENT_MAX_RENDER_TARGETS)
				return;

			auto& clear_color = g_renderer_state.clear_color;
			g_renderer_state.clear_rt_index = desc.render_target_index;

			for (u8 i = 0; i < 4; ++i)
				clear_color[i] = desc.value[i];

			g_renderer_state.dirty_flags |= (u32)renderer_dirty_flags::clear_color;
		}

		void renderer_set_clear_depth(const clear_depth_desc& desc) {
			g_renderer_state.clear_depth_value = desc.depth;
			g_renderer_state.clear_stencil_value = desc.stencil;

			g_renderer_state.dirty_flags |= (u32)renderer_dirty_flags::clear_depth;
			if (desc.clear_stencil)
				g_renderer_state.dirty_flags |= (u32)renderer_dirty_flags::clear_stencil;
		}

		void draw() {
			renderer__submit_render_state();

			//const auto ctx = get_state()->contexts;
		}
	}
}