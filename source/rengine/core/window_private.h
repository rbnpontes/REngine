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
		struct window_state {
			u32 next_id;
			u8 count;
		};

		extern window_state g_window_state;
		extern eastl::array<window_data, CORE_WINDOWS_MAX_ALLOWED> g_windows;
		static c_str g_window_id_prop_key = "rengine.window_id";

		u8 window__decode_id(const window_t& id);
		window_t window__encode_id(const u8& slot_idx);
		u8 window__assert_id(const window_t& id);
		window_data& window__get_data(const window_t& id);
		window_t window__find_id_from_sdl_wnd(void* sdl_wnd);
		void window__init();
		void window__deinit();
		u8 window__get_free_slot_idx();
		window_t window__alloc(u8& slot_idx);
		window_t window__idx_to_id(const window_t& idx);

		void window__handle_sdl_event(const window_t& id, SDL_Event& evt);
	}
}