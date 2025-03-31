#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <rengine/math/math-types.h>

namespace rengine {
    namespace core {
        using namespace math;
        struct window_desc_t {
            c_str title;
            irect_t bounds;
            bool visible;
        };

        R_EXPORT window_t window_create(c_str title, u32 width, u32 height);
        R_EXPORT void window_show(window_t window);
        R_EXPORT void window_hide(window_t window);
        R_EXPORT void window_destroy(window_t window);
        R_EXPORT void window_set_title(window_t window, c_str title);
        R_EXPORT void window_set_size(window_t window, ivec2_t size);
        R_EXPORT void window_set_position(window_t window, ivec2_t position);
        R_EXPORT window_desc_t window_get_desc(window_t window);
        R_EXPORT void window_poll_events();
        R_EXPORT u8 window_count();
        R_EXPORT bool window_is_destroyed(window_t window);
    }
}