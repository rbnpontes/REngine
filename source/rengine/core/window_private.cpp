#include "./window_private.h"
#include "./window_graphics_private.h"

#include "../events/events.h"
#include "../exceptions.h"
#include "../strings.h"

namespace rengine {
	namespace core {
        window_state g_window_state = {};
        eastl::array<window_data, CORE_WINDOWS_MAX_ALLOWED> g_windows = {};

        u8 window__decode_id(const window_t& id) {
            return static_cast<u8>(id % CORE_WINDOWS_MAX_ALLOWED);
        }

        window_t window__encode_id(const u8& slot_idx) {
            return (g_window_state.next_id * CORE_WINDOWS_MAX_ALLOWED) + slot_idx;
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
            if (!sdl_wnd)
                return core::no_window;

            const auto wnd_prop = SDL_GetWindowProperties(static_cast<SDL_Window*>(sdl_wnd));
            const auto window_id = (window_t)SDL_GetNumberProperty(wnd_prop, 
                g_window_id_prop_key, 
                core::no_window);
            return window_id;
        }

        void window__init() {
            SDL_Init(SDL_INIT_AUDIO);
        }

        void window__deinit() {
            for (u8 i = 0; i < CORE_WINDOWS_MAX_ALLOWED; ++i) {
                const auto wnd = g_windows[i];
                if (!wnd.owner)
                    continue;

                window_hide(wnd.id);
            }

            SDL_Quit();
        }

        u8 window__get_free_slot_idx() {
            for (u8 i = 0; i < CORE_WINDOWS_MAX_ALLOWED; ++i) {
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
            ++g_window_state.next_id;

            return id;
        }

        window_t window__idx_to_id(const window_t& idx)
        {
            return g_windows[idx].id;
        }

        void window__handle_sdl_event(const window_t& id, SDL_Event& evt)
        {
            SDL_Window* sdl_wnd = SDL_GetWindowFromEvent(&evt);
            switch (evt.type)
            {
            case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
            {
                EVENT_EMIT(window, quit)(id, sdl_wnd);
                window_destroy(id);
            }
                break;
            case SDL_EVENT_WINDOW_RESIZED:
            {
                EVENT_EMIT(window, resize)(id, sdl_wnd);
                window__resize_swapchain(id);
            }
                break;
            }
        }
	}
}