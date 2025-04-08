#include "./diligent_private.h"
#include "./graphics_private.h"
#include "./graphics_utils_private.h"
#include "./graphics.h"

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
			g_graphics_state.factory = factory;

			Diligent::EngineD3D11CreateInfo create_info = {};
			utils::setup_engine_create_info(factory, desc.adapter_id, desc.backend, create_info);

			g_graphics_state.num_contexts = std::max(create_info.NumImmediateContexts, 1u) + create_info.NumDeferredContexts;
			g_graphics_state.contexts = core::alloc_array_alloc<Diligent::IDeviceContext*>(g_graphics_state.num_contexts);
			memset(g_graphics_state.contexts, 0x0, sizeof(Diligent::IDeviceContext) * g_graphics_state.num_contexts);
			factory->CreateDeviceAndContextsD3D11(create_info, &g_graphics_state.device, g_graphics_state.contexts);
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

		void assert_diligent_objects()
		{
			for (u8 i = 0; i < g_graphics_state.num_contexts; ++i) {
				if (g_graphics_state.contexts[i])
					continue;
				throw graphics_exception(strings::exceptions::g_graphics_fail_to_create_g_objects);
			}

			if (!g_graphics_state.device)
				throw graphics_exception(strings::exceptions::g_graphics_fail_to_create_g_objects);
		}

		Diligent::ISwapChain* allocate_window_swapchain__d3d11(const core::window_t& window_id)
		{
#ifdef FEATURE_BACKEND_D3D11
			auto factory = (Diligent::IEngineFactoryD3D11*)g_graphics_state.factory;
			auto window_desc = core::window_get_desc(window_id);
			Diligent::NativeWindow native_window;
			Diligent::SwapChainDesc desc;
			Diligent::ISwapChain* swap_chain;

			desc.Width = window_desc.bounds.size.x;
			desc.Height = window_desc.bounds.size.y;
			desc.ColorBufferFormat = (Diligent::TEXTURE_FORMAT)get_default_backbuffer_format();
			desc.DepthBufferFormat = Diligent::TEX_FORMAT_UNKNOWN;

			core::window__fill_native_window(window_id, native_window);

			factory->CreateSwapChainD3D11(g_graphics_state.device, g_graphics_state.contexts[0], desc, {}, native_window, &swap_chain);
			return swap_chain;
#endif
		}
		Diligent::ISwapChain* allocate_window_swapchain__d3d12(const core::window_t& window_id)
		{
			return null;
		}
		
		Diligent::ISwapChain* allocate_window_swapchain__vk(const core::window_t& window_id)
		{
			return null;
		}

		Diligent::ISwapChain* allocate_window_swapchain__webgpu(const core::window_t& window_id)
		{
			return null;
		}
		
		Diligent::ISwapChain* allocate_window_swapchain__opengl(const core::window_t& window_id)
		{
			throw graphics_exception(strings::exceptions::g_graphics_opengl_doesnt_support_swapchain);
		}
    }
}