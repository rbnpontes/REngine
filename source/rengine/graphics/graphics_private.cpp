#include "./graphics_private.h"
#include "./diligent_private.h"

#include "../defines.h"
#include "../exceptions.h"
#include "../strings.h"

#include <fmt/format.h>
#include <thread>

namespace rengine {
	namespace graphics {
		graphics_state g_state = {};

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
			const auto initialized = g_state.factory != null
				&& g_state.contexts != null
				&& g_state.device != null
				&& g_state.num_contexts > 0;
			if (initialized)
				return;

			throw graphics_exception(strings::exceptions::g_graphics_not_initialized);
		}

		void init(const graphics_init_desc& desc)
		{
			assert_backend(desc.backend);
			g_state.backend = desc.backend;

			switch (desc.backend)
			{
			case backend::d3d11:
				init_d3d11(desc);
				break;
			case backend::d3d12:
				init_d3d12(desc);
				break;
			case backend::vulkan:
				init_vk(desc);
				break;
			case backend::webgpu:
				init_webgpu(desc);
				break;
			case backend::opengl:
				init_opengl(desc);
				break;
			}
		}

		void deinit()
		{
			assert_initialization();

			for(u32 i = 0; i < g_state.num_contexts; ++i)
				g_state.contexts[i]->Release();
			g_state.device->Release();
			g_state.factory->Release();

			core::alloc_free(g_state.contexts);
		}

		void begin() {

		}

		void end() {

		}
	}
}