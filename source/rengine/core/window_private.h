#pragma once
#include "../base_private.h"
#include "./window.h"

#include <SDL3/SDL.h>
#include <EASTL/array.h>
#include <EASTL/shared_ptr.h>

namespace rengine {
	namespace core {
		struct window_data {
			SDL_Window*				owner;
			ptr 					swap_chain;
			window_t                id;
		};

		extern u32 g_next_id;
		extern eastl::array<window_data, MAX_ALLOWED_WINDOWS> g_windows;

		u8 window__decode_id(const window_t& id);
		window_t window__encode_id(const u8& slot_idx);
		u8 window__assert_id(const window_t& id);
		window_data& window__get_data(const window_t& id);
		window_t window__find_id_from_sdl_wnd(void* sdl_wnd);
		void window__init();
		void window__deinit();
		u8 window__get_free_slot_idx();
		window_t window__alloc(u8& slot_idx);
	}
}