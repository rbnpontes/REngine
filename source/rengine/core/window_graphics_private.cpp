#include "./window_graphics_private.h"
#include "./window_private.h"
#include "../exceptions.h"

#include <SDL3/SDL.h>

namespace rengine {
	namespace core {
		void window__fill_native_window(const window_t& window_id, Diligent::NativeWindow& native_window) {
			const auto& data = window__get_data(window_id);
			const auto properties = SDL_GetWindowProperties(data.owner);
#ifdef PLATFORM_WINDOWS
			native_window.hWnd = SDL_GetPointerProperty(properties, SDL_PROP_WINDOW_WIN32_HWND_POINTER, null);
#else
			throw not_implemented_exception();
#endif
		}

		void window__put_swapchain(const window_t& window_id, Diligent::ISwapChain* swapchain) {
			auto& data = window__get_data(window_id);
			data.swap_chain = swapchain;
			swapchain->AddRef();
		}
		
		bool window__has_swapchain(const window_t& window_id)
		{
			const auto& data = window__get_data(window_id);
			return static_cast<Diligent::ISwapChain*>(data.swap_chain) != null;
		}
		
		Diligent::ISwapChain* window__get_swapchain(const window_t& window_id)
		{
			auto& data = window__get_data(window_id);
			return static_cast<Diligent::ISwapChain*>(data.swap_chain);
		}

		void window__resize_swapchain(const window_t& window_id)
		{
			const auto& data = window__get_data(window_id);
			i32 w, h;
			SDL_GetWindowSize(data.owner, &w, &h);

			if (w == 0 || h == 0)
				return;

			const auto swapchain = static_cast<Diligent::ISwapChain*>(data.swap_chain);
			swapchain->Resize(w, h);
		}

		void window__release_swapchain(const window_t& window_id)
		{
			auto& data = window__get_data(window_id);
			static_cast<Diligent::ISwapChain*>(data.swap_chain)->Release();
			data.swap_chain = null;
		}

		void window__present_swapchains()
		{
			for (u8 i = 0; i < CORE_WINDOWS_MAX_ALLOWED; ++i) {
				const auto& data = g_windows[i];
				if (!data.swap_chain)
					continue;
				static_cast<Diligent::ISwapChain*>(data.swap_chain)->Present();
			}
		}
	}
}