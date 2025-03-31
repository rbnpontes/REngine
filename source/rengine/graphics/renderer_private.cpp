#include "./renderer_private.h"
#include "./graphics_private.h"

#include "../exceptions.h"
#include "../strings.h"

#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		renderer_state g_renderer_state = {};

		void renderer__reset_state() {
			g_renderer_state = {};
		}

		void renderer__set_render_targets()
		{
			const auto ctx = g_graphics_state.contexts[0];

			if ((g_renderer_state.dirty_flags & (u32)renderer_dirty_flags::render_targets) == 0)
				return;
			g_renderer_state.dirty_flags &= (u32)renderer_dirty_flags::render_targets;

			ctx->SetRenderTargets(g_renderer_state.num_render_targets,
				g_renderer_state.render_targets,
				g_renderer_state.depth_stencil,
				Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
		}

		void renderer__submit_render_state()
		{
			const auto ctx = g_graphics_state.contexts[0];
			if (g_renderer_state.dirty_flags == (u32)renderer_dirty_flags::none)
				return;

			renderer__set_render_targets();
		}

		void renderer__assert_render_target_idx(u8 idx)
		{
			if (idx >= DILIGENT_MAX_RENDER_TARGETS)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_renderer_rt_idx_grt_than_max, DILIGENT_MAX_RENDER_TARGETS).c_str()
				);

			if (idx < g_graphics_state.num_contexts)
				return;

			throw graphics_exception(
				fmt::format(strings::exceptions::g_renderer_rt_idx_grt_than_set, 
					idx, 
					g_renderer_state.num_render_targets).c_str()
			);
		}
	}
}