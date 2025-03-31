#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

namespace rengine {
    namespace graphics {
        struct clear_color_desc {
            u8 render_target_index{ 0 };
            float value[4] RENDERER_DEFAULT_CLEAR_COLOR;
        };
        struct clear_depth_desc {
            float   depth{ 0.0f };
            u8      stencil{ 0 };
            bool    clear_stencil{ false };
        };

        R_EXPORT void renderer_set_window(core::window_t window_id);
        R_EXPORT void renderer_set_clear_color(const clear_color_desc& desc);
        R_EXPORT void renderer_set_clear_depth(const clear_depth_desc& desc);
        R_EXPORT void renderer_flush();
        R_EXPORT void renderer_draw();
    }
}