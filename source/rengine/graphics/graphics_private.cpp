#include "./graphics_private.h"
#include "./diligent_private.h"
#include "./renderer_private.h"
#include "./renderer.h"

#include "../rengine_private.h"
#include "../core/window_graphics_private.h"

#include "../defines.h"
#include "../exceptions.h"
#include "../strings.h"

#include <fmt/format.h>
#include <thread>

namespace rengine {
	namespace graphics {
		graphics_state g_graphics_state = {};

		typedef void(*init_call_fn)(const graphics_init_desc&);
		typedef Diligent::ISwapChain*(*allocate_swapchain_call_fn)(const core::window_t&);

		void assert_backend(backend value) {
			c_str backend_str = strings::g_backend_strings[(u8)backend::max_backend];
			if (value < backend::max_backend)
				backend_str = strings::g_backend_strings[(u8)value];

#ifdef FEATURE_BACKEND_D3D11
			if (value == backend::d3d11)
				return;
#endif
#ifdef FEATURE_BACKEND_D3D12
			if (value == backend::d3d12)
				return;
#endif
#ifdef FEATURE_BACKEND_VULKAN
			if (value == backend::vulkan)
				return;
#endif
#ifdef FEATURE_BACKEND_WEBGPU
			if (value == backend::webgpu)
				return;
#endif
#ifdef FEATURE_BACKEND_OPENGL
			if (value == backend::opengl)
				return;
#endif

			throw graphics_exception(
				fmt::format(strings::exceptions::g_graphics_unsupported_backend, backend_str).c_str()
			);
		}

		void assert_initialization() {
			const auto initialized = g_graphics_state.factory != null
				&& g_graphics_state.contexts != null
				&& g_graphics_state.device != null
				&& g_graphics_state.num_contexts > 0;
			if (initialized)
				return;

			throw graphics_exception(strings::exceptions::g_graphics_not_initialized);
		}

		void init(const graphics_init_desc& desc)
		{
			init_call_fn init_calls[] = {
				init_d3d11,
				init_d3d12,
				init_vk,
				init_webgpu,
				init_opengl
			};

			assert_backend(desc.backend);
			g_graphics_state.backend = desc.backend;

			init_calls[(u8)g_graphics_state.backend](desc);
			assert_diligent_objects();
		}

		void deinit()
		{
			assert_initialization();

			for(u32 i = 0; i < g_graphics_state.num_contexts; ++i)
				g_graphics_state.contexts[i]->Release();
			g_graphics_state.device->Release();
			g_graphics_state.factory->Release();

			core::alloc_free(g_graphics_state.contexts);
		}

		void begin() {
			renderer__reset_state();

			const auto window_id = g_engine_state.curr_wnd;
			if (window_id == core::no_window)
				return;

			allocate_swapchain(window_id);
			renderer_set_window(window_id);
		}

		void end() {
			const auto window_id = g_engine_state.curr_wnd;
			const auto swap_chain = core::window__get_swapchain(window_id);
			if (!swap_chain)
				return;

			swap_chain->Present();
		}

		void allocate_swapchain(const core::window_t& window_id)
		{
			assert_initialization();
			if (core::window__has_swapchain(window_id))
				return;

			allocate_swapchain_call_fn allocate_calls[] = {
				allocate_window_swapchain__d3d11,
				allocate_window_swapchain__d3d12,
				allocate_window_swapchain__vk,
				allocate_window_swapchain__webgpu,
				allocate_window_swapchain__opengl
			};

			const auto swapchain = allocate_calls[(u8)g_graphics_state.backend](window_id);
			if (!swapchain)
				throw graphics_exception(strings::exceptions::g_graphics_fail_to_create_swapchain);

			core::window__put_swapchain(window_id, swapchain);
		}
	}
}