#include "./renderer_private.h"
#include "./graphics_private.h"

namespace rengine {
	namespace graphics {
		renderer_state g_renderer_state = {};

		void renderer__reset_state() {
			g_renderer_state = {};
		}
		void renderer__submit_render_state()
		{
			const auto ctx = g_graphics_state.contexts[0];
			if (g_renderer_state.dirty_flags == (u32)renderer_dirty_flags::none)
				return;

			if ((g_renderer_state.dirty_flags & (u32)renderer_dirty_flags::render_targets) != 0) {
				ctx->SetRenderTargets(g_renderer_state.num_render_targets,
					g_renderer_state.render_target,
					g_renderer_state.depth_stencil,
					Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
			}

			if ((g_renderer_state.dirty_flags & (u32)renderer_dirty_flags::clear_color) != 0) {
				ctx->ClearRenderTarget(g_renderer_state.render_target[0],
					g_renderer_state.clear_color,
					Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
			}

			if ((g_renderer_state.dirty_flags & (u32)renderer_dirty_flags::clear_depth) != 0) {
				const auto clear_stencil = (g_renderer_state.dirty_flags & (u32)renderer_dirty_flags::clear_stencil) != 0;
				auto flags = Diligent::CLEAR_DEPTH_FLAG_NONE;
				if (clear_stencil)
					flags |= Diligent::CLEAR_STENCIL_FLAG;

				ctx->ClearDepthStencil(g_renderer_state.depth_stencil,
					flags,
					g_renderer_state.clear_depth_value,
					g_renderer_state.clear_stencil_value,
					Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
			}

			g_renderer_state.dirty_flags = (u32)renderer_dirty_flags::none;
		}
	}
}