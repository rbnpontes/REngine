#pragma once
#include "../base_private.h"
#include "./graphics_private.h"
#include <SwapChain.h>

namespace rengine {
    namespace graphics {
		void init_d3d11(const graphics_init_desc& desc);
		void init_d3d12(const graphics_init_desc& desc);
		void init_vk(const graphics_init_desc& desc);
		void init_webgpu(const graphics_init_desc& desc);
		void init_opengl(const graphics_init_desc& desc);
		void assert_diligent_objects();

		Diligent::ISwapChain* allocate_window_swapchain__d3d11(const core::window_t& window_id);
		Diligent::ISwapChain* allocate_window_swapchain__d3d12(const core::window_t& window_id);
		Diligent::ISwapChain* allocate_window_swapchain__vk(const core::window_t& window_id);
		Diligent::ISwapChain* allocate_window_swapchain__webgpu(const core::window_t& window_id);
		Diligent::ISwapChain* allocate_window_swapchain__opengl(const core::window_t& window_id);
    }
}