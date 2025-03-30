#include "./window_private.h"
#include "../exceptions.h"
#include "../strings.h"

namespace rengine {
	namespace core {
        u32 g_next_id = 0;
        eastl::array<window_data, MAX_ALLOWED_WINDOWS> g_windows = {};

        u8 window__decode_id(const window_t& id) {
            return static_cast<u8>(id % MAX_ALLOWED_WINDOWS);
        }

        window_t window__encode_id(const u8& slot_idx) {
            return (g_next_id * MAX_ALLOWED_WINDOWS) + slot_idx;
        }

        u8 window__assert_id(const window_t& id) {
            const auto idx = window__decode_id(id);
            const auto& wnd = g_windows[idx];

            if (wnd.id != id || wnd.owner == null)
                throw window_exception(strings::exceptions::g_window_invalid_id);

            return idx;
        }

        window_data& window__get_data(const window_t& id) {
            const auto idx = window__assert_id(id);
            return g_windows[idx];
        }

        window_t window__find_id_from_sdl_wnd(void* sdl_wnd) {
            for (u8 i = 0; i < MAX_ALLOWED_WINDOWS; ++i) {
                const auto& wnd = g_windows[i];
                if (wnd.owner == sdl_wnd)
                    return wnd.id;
            }

            return -1;
        }

        void window__init() {
            SDL_Init(SDL_INIT_AUDIO);
        }

        void window__deinit() {
            for (u8 i = 0; i < MAX_ALLOWED_WINDOWS; ++i) {
                const auto wnd = g_windows[i];
                if (!wnd.owner)
                    continue;

                window_hide(wnd.id);
            }

            SDL_Quit();
        }

        u8 window__get_free_slot_idx() {
            for (u8 i = 0; i < MAX_ALLOWED_WINDOWS; ++i) {
                if (!g_windows[i].owner)
                    return i;
            }
            return MAX_U8_VALUE;
        }

        window_t window__alloc(u8& slot_idx) {
            slot_idx = window__get_free_slot_idx();
            if (slot_idx == MAX_U8_VALUE)
                throw window_exception(strings::exceptions::g_window_reached_max_created_windows);

            const auto id = window__encode_id(slot_idx);
            g_windows[slot_idx] = {
                null,
                null,
                id
            };
            ++g_next_id;

            return id;
        }
	}
}