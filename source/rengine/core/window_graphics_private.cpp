#include "./window_graphics_private.h"
#include "./window_private.h"
#include "../exceptions.h"

#include <SDL3/SDL.h>

namespace rengine {
	namespace core {
		void window__fill_native_window(const window_t& window_id, Diligent::NativeWindow& native_window) {
			const auto& data = window__get_data(window_id);
			const auto properties = SDL_GetWindowProperties((SDL_Window*)data.owner);
#ifdef PLATFORM_WINDOWS
			native_window.hWnd = SDL_GetPointerProperty(properties, SDL_PROP_WINDOW_CREATE_WIN32_HWND_POINTER, null);
#else
			throw not_implemented_exception();
#endif
		}

		void window__put_swapchain(const window_t& window_id, Diligent::ISwapChain* swapchain) {
			auto& data = window__get_data(window_id);
			data.swap_chain = swapchain;
		}
		bool window__has_swapchain(const window_t& window_id)
		{
			const auto& data = window__get_data(window_id);
			return static_cast<Diligent::ISwapChain*>(data.swap_chain) != null;
		}
		void window__release_swapchain(const window_t& window_id)
		{
			auto& data = window__get_data(window_id);
			static_cast<Diligent::ISwapChain*>(data.swap_chain)->Release();
			data.swap_chain = null;
		}
	}
}