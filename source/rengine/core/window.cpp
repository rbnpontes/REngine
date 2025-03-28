#include "./window_private.h"
#include "../defines.h"
#include "../exceptions.h"

#include <SDL3/SDL.h>
#include <EASTL/array.h>
#include <EASTL/shared_ptr.h>
#include <SwapChain.h>

namespace rengine {
    namespace core {
        typedef eastl::shared_ptr<window_data_t> window_ptr_t;
        static u32 g_next_id = 0;
        static eastl::array<window_data_t, MAX_ALLOWED_WINDOWS> g_windows = {};

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
                throw window_exception("Invalid window id");

            return idx;
        }

        window_data_t& window__get_data(const window_t& id) {
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
            SDL_Init(SDL_INIT_AUDIO | SDL_INIT_VIDEO);
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
            return 0xFF;
        }

        window_t window__alloc(u8& slot_idx) {
            slot_idx = window__get_free_slot_idx();
            if (slot_idx == 0xFF)
                throw window_exception("Reached max of created windows.");
            
            const auto id = window__encode_id(slot_idx);
            g_windows[slot_idx] = {
                null,
                null,
                id
            };
            ++g_next_id;
            
            return id;
        }

        window_t window_create(c_str title, u32 width, u32 height)
        {
            u8 wnd_idx;
            auto wnd_id = window__alloc(wnd_idx);
            auto sdl_wnd = SDL_CreateWindow(title, width, height, 0);

            g_windows[wnd_idx].owner = sdl_wnd;
            return wnd_id;
        }
        
        void window_show(window_t id) {
            const auto& wnd = window__get_data(id);
            SDL_ShowWindow(wnd.owner);
        }
        
        void window_hide(window_t id)
        {
            const auto& wnd = window__get_data(id);
            SDL_HideWindow(wnd.owner);
        }

        void window_destroy(window_t id)
        {
            const auto idx = window__assert_id(id);
            const auto& wnd = g_windows[idx];

            if(wnd.swap_chain != null)
                wnd.swap_chain->Release();
            SDL_DestroyWindow(wnd.owner);
            g_windows[idx] = {};
        }

        void window_set_title(window_t id, c_str title)
        {
            const auto& wnd = window__get_data(id);
            SDL_SetWindowTitle(wnd.owner, title);
        }

        void window_set_size(window_t id, ivec2_t size)
        {
            const auto& wnd = window__get_data(id);
            SDL_SetWindowSize(wnd.owner, size.x, size.y);
        }

        void window_set_position(window_t id, ivec2_t position)
        {
            const auto& wnd = window__get_data(id);
            SDL_SetWindowPosition(wnd.owner, position.x, position.y);
        }

        window_desc_t window_get_desc(window_t id)
        {
            const auto& wnd = window__get_data(id);
            int x, y, w, h;

            SDL_GetWindowPosition(wnd.owner, &x, &y);
            SDL_GetWindowSize(wnd.owner, &w, &h);

            return {
                SDL_GetWindowTitle(wnd.owner),
                { {x, y}, {w, h} },
                (SDL_GetWindowFlags(wnd.owner) & SDL_WINDOW_HIDDEN) != 0
            };
        }

        void window_poll_events()
        {
            SDL_Event evt;
            while (SDL_PollEvent(&evt) != 0) {
                if (evt.type != SDL_EVENT_QUIT)
                    continue;

                window_t id = window__find_id_from_sdl_wnd(
                    SDL_GetWindowFromEvent(&evt)
                );

                if (id == -1)
                    continue;

                window_destroy(id);
            }
        }
    }
}