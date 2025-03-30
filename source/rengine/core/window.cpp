#include "./window_private.h"
#include "./window_graphics_private.h"

#include "../defines.h"
#include "../exceptions.h"

#include <SDL3/SDL.h>
#include <EASTL/array.h>
#include <EASTL/shared_ptr.h>

namespace rengine {
    namespace core {
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

            if (wnd.swap_chain != null)
                window__release_swapchain(id);
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