#pragma once
#include "../base_private.h"

#include <NativeWindow.h>
#include <SwapChain.h>

namespace rengine {
    namespace core {
        void window__fill_native_window(const window_t& window_id, Diligent::NativeWindow& native_window);
        void window__put_swapchain(const window_t& window_id, Diligent::ISwapChain* swapchain);
        bool window__has_swapchain(const window_t& window_id);
        Diligent::ISwapChain* window__get_swapchain(const window_t& window_id);
        void window__release_swapchain(const window_t& window_id);
    }
}