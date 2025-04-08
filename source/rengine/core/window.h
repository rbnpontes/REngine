#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <rengine/math/math-types.h>

namespace rengine {
    namespace core {
        using namespace math;
        struct window_desc_t {
            c_str title;
            irect bounds;
            bool visible;
        };

        R_EXPORT window_t window_create(c_str title, u32 width, u32 height);
        R_EXPORT void window_show(const window_t& window);
        R_EXPORT void window_hide(const window_t& window);
        R_EXPORT void window_destroy(const window_t& window);
        R_EXPORT void window_set_title(const window_t& window, c_str title);
        R_EXPORT void window_set_size(const window_t& window, ivec2 size);
        R_EXPORT void window_set_position(const window_t& window, ivec2 position);
        R_EXPORT window_desc_t window_get_desc(const window_t& window);
        R_EXPORT math::uvec2 window_get_size(const window_t& window);
        R_EXPORT void window_poll_events();
        R_EXPORT u8 window_count();
        R_EXPORT bool window_is_destroyed(const window_t& window);
    }
}