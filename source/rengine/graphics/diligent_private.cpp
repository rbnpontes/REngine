#include "./diligent_private.h"
#include "./graphics_private.h"
#include "./graphics_utils_private.h"

#include "../exceptions.h"
#include "../core/allocator.h"
#include "../core/window_private.h"
#include "../core/window_graphics_private.h"
#include "../io/io.h"

#ifdef FEATURE_BACKEND_D3D11
#include <EngineFactoryD3D11.h>
#endif
#ifdef FEATURE_BACKEND_D3D12
#include <EngineFactoryD3D12.h>
#endif
#ifdef FEATURE_BACKEND_VULKAN
#include <EngineFactoryVk.h>
#endif
#ifdef FEATURE_BACKEND_WEBGPU
#include <EngineFactoryWebGPU.h>
#endif
#ifdef FEATURE_BACKEND_OPENGL
#include <EngineFactoryOpenGL.h>
#endif

#include <fmt/format.h>
#include <utility>

namespace rengine {
    namespace graphics {

		void init_d3d11(const graphics_init_desc& desc)
		{
#ifdef FEATURE_BACKEND_D3D11
			const auto factory = Diligent::GetEngineFactoryD3D11();
			factory->SetMessageCallback(utils::diligent_dbg_message_helper);
			g_state.factory = factory;

			Diligent::EngineD3D11CreateInfo create_info = {};
			utils::setup_engine_create_info(factory, desc.adapter_id, desc.backend, create_info);

			g_state.num_contexts = std::max(create_info.NumImmediateContexts, 1u) + create_info.NumDeferredContexts;
			g_state.contexts = core::alloc_array_alloc<Diligent::IDeviceContext*>(g_state.num_contexts);
			memset(g_state.contexts, 0x0, sizeof(Diligent::IDeviceContext) * g_state.num_contexts);
			factory->CreateDeviceAndContextsD3D11(create_info, &g_state.device, g_state.contexts);
#endif
		}

		void init_d3d12(const graphics_init_desc& desc)
		{
#ifdef FEATURE_BACKEND_D3D12
			throw not_implemented_exception();
#endif
		}

		void init_vk(const graphics_init_desc& desc)
		{
#ifdef FEATURE_BACKEND_VULKAN
			throw not_implemented_exception();
#endif
		}

		void init_webgpu(const graphics_init_desc& desc)
		{
#ifdef FEATURE_BACKEND_WEBGPU
			throw not_implemented_exception();
#endif
		}

		void init_opengl(const graphics_init_desc& desc)
		{
#ifdef FEATURE_BACKEND_OPENGL
			throw not_implemented_exception();
#endif
		}

		void allocate_window_swapchain(const core::window_t& window_id)
		{
			assert_initialization();

			if (core::window__has_swapchain(window_id)) {
				io::logger_warn(strings::logs::g_graphics_tag,
					fmt::format(strings::logs::g_graphics_swapchain_has_been_created, window_id).c_str());
				return;
			}

			switch (g_state.backend)
			{
			case backend::d3d11:
				allocate_window_swapchain__d3d11(window_id);
				break;
			case backend::d3d12:
				allocate_window_swapchain__d3d12(window_id);
				break;
			case backend::vulkan:
				allocate_window_swapchain__vk(window_id);
				break;
			case backend::webgpu:
				allocate_window_swapchain__webgpu(window_id);
				break;
			case backend::opengl:
				allocate_window_swapchain__opengl(window_id);
				break;
			}
		}

		void allocate_window_swapchain__d3d11(const core::window_t& window_id)
		{
#ifdef FEATURE_BACKEND_D3D11
			auto factory = (Diligent::IEngineFactoryD3D11*)g_state.factory;
			auto window_desc = core::window_get_desc(window_id);
			Diligent::NativeWindow native_window;
			Diligent::SwapChainDesc desc;
			Diligent::ISwapChain* swap_chain;

			desc.Width = window_desc.bounds.size.x;
			desc.Height = window_desc.bounds.size.y;
			desc.ColorBufferFormat = Diligent::TEX_FORMAT_RGBA8_UNORM;
			desc.DepthBufferFormat = Diligent::TEX_FORMAT_D16_UNORM;

			core::window__fill_native_window(window_id, native_window);

			factory->CreateSwapChainD3D11(g_state.device, g_state.contexts[0], desc, {}, native_window, &swap_chain);
			core::window__put_swapchain(window_id, swap_chain);
#endif
		}
		void allocate_window_swapchain__d3d12(const core::window_t& window_id)
		{
		}
		void allocate_window_swapchain__vk(const core::window_t& window_id)
		{
		}
		void allocate_window_swapchain__webgpu(const core::window_t& window_id)
		{
		}
		void allocate_window_swapchain__opengl(const core::window_t& window_id)
		{
			throw graphics_exception(strings::exceptions::g_graphics_opengl_doesnt_support_swapchain);
		}
    }
}