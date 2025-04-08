#include "./renderer_private.h"
#include "./graphics_private.h"
#include "./render_target_manager_private.h"

#include "../exceptions.h"
#include "../strings.h"

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

		void renderer__reset_state() {
			g_renderer_state.depth_stencil = no_render_target;
			g_renderer_state.dirty_flags = (u32)renderer_dirty_flags::none;
			g_renderer_state.viewport = math::urect::zero;
			g_renderer_state.num_render_targets = 0;
			g_renderer_state.render_targets.fill(no_render_target);
		}

		void renderer__set_render_targets()
		{
			const auto ctx = g_graphics_state.contexts[0];
			const auto test_flags = g_renderer_state.dirty_flags &
				((u32)renderer_dirty_flags::render_targets | (u32)renderer_dirty_flags::depth_stencil);

			if (test_flags == 0)
				return;

			g_renderer_state.dirty_flags ^= (u32)renderer_dirty_flags::render_targets;
			g_renderer_state.dirty_flags ^= (u32)renderer_dirty_flags::depth_stencil;

			Diligent::ITextureView* render_targets[GRAPHICS_MAX_RENDER_TARGETS] = {};
			Diligent::ITextureView* depth_stencil = null;

			for (u8 i = 0; i < g_renderer_state.num_render_targets; ++i) {
				Diligent::ITexture* backbuffer = null;
				render_targets[i] = null;

				render_target_mgr__get_internal_handles(g_renderer_state.render_targets[i],
					&backbuffer, null);

				if (backbuffer)
					render_targets[i] = backbuffer->GetDefaultView(Diligent::TEXTURE_VIEW_RENDER_TARGET);
			}

			if (g_renderer_state.depth_stencil != no_render_target) {
				Diligent::ITexture* depthbuffer = null;
				render_target_mgr__get_internal_handles(g_renderer_state.depth_stencil,
					null,
					&depthbuffer);
				depth_stencil = depthbuffer->GetDefaultView(Diligent::TEXTURE_VIEW_DEPTH_STENCIL);
			}

			ctx->SetRenderTargets(g_rt_mgr_state.count,
				render_targets,
				depth_stencil,
				Diligent::RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
		}

		void renderer__set_viewport()
		{
			if ((g_renderer_state.dirty_flags & (u32)renderer_dirty_flags::viewport) == 0)
				return;
			g_renderer_state.dirty_flags ^= (u32)renderer_dirty_flags::viewport;

			const auto ctx = g_graphics_state.contexts[0];
			const auto& viewport = g_renderer_state.viewport;
			auto rt_size = viewport.size;

			if (g_renderer_state.num_render_targets > 0)
				render_target_mgr__get_size(g_renderer_state.render_targets[0], &rt_size);

			Diligent::Viewport view;
			view.TopLeftX = viewport.position.x;
			view.TopLeftY = viewport.position.y;
			view.Width = viewport.size.x;
			view.Height = viewport.size.y;
			view.MinDepth = 0;
			view.MaxDepth = 1;
			ctx->SetViewports(1, &view, rt_size.x, rt_size.y);
		}

		void renderer__submit_render_state()
		{
			const auto ctx = g_graphics_state.contexts[0];
			if (g_renderer_state.dirty_flags == (u32)renderer_dirty_flags::none)
				return;

			renderer__set_render_targets();
			renderer__set_viewport();
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