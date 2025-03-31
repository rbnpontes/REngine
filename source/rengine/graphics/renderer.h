#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

namespace rengine {
	namespace graphics {
		struct clear_desc {
			u8 render_target_index{ 0 };
			u8 stencil{ 1 };
			bool clear_depth{ false };
			bool clear_stencil{ false };
			float color[4] RENDERER_DEFAULT_CLEAR_COLOR;
			float depth{ 0.0f };
		};

		R_EXPORT void renderer_set_window(core::window_t window_id);
		R_EXPORT void renderer_clear(const clear_desc& desc);
		R_EXPORT void renderer_flush();
		R_EXPORT void renderer_draw();
	}
}