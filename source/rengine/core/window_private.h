#pragma once
#include "./window.h"

#include <SDL3/SDL.h>
#include <EASTL/array.h>
#include <EASTL/shared_ptr.h>
#include <SwapChain.h>

namespace rengine {
	namespace core {
		struct window_data_t {
			SDL_Window* owner;
			Diligent::ISwapChain* swap_chain;
			window_t                id;
		};

		u8 window__decode_id(const window_t& id);
		window_t window__encode_id(const u8& slot_idx);
		u8 window__assert_id(const window_t& id);
		window_data_t& window__get_data(const window_t& id);
		window_t window__find_id_from_sdl_wnd(void* sdl_wnd);
		void window__init();
		void window__deinit();
		u8 window__get_free_slot_idx();
		window_t window__alloc(u8& slot_idx);
	}
}