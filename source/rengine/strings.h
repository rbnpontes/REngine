#pragma once
#include <rengine/types.h>

namespace rengine {
    namespace strings {
        constexpr static c_str g_backend_strings[] = {
            "d3d11",
            "d3d12",
            "vulkan",
            "webgpu",
            "opengl",
            "unknow"
        };

        namespace logs {
            constexpr static c_str g_graphics_tag = "graphics";
            constexpr static c_str g_diligent_tag = "diligent";

            constexpr static c_str g_logger_fmt = "[{0}/{1}/{2} {3}:{4}:{5}][{6}][7]: {8}";

            constexpr static c_str g_graphics_invalid_adapter_id = "Invalid adapter id {0}. Engine will try to select a best match device";
            constexpr static c_str g_graphics_no_suitable_device_found = "No suitable device found, using first available.";
            constexpr static c_str g_graphics_swapchain_has_been_created = "SwapChain has been created for window {0}";
            constexpr static c_str g_graphics_diligent_dbg_fmt = "{0} | Function: {1} | File: {2} | Line: {3}";
        }

        namespace exceptions {
            constexpr static c_str g_null_object = "{0} is null";

            constexpr static c_str g_window_invalid_id = "Invalid window id";
            constexpr static c_str g_window_reached_max_created_windows = "Reached max of created windows";

            constexpr static c_str g_graphics_unsupported_backend = "Unsupported graphics backend {0} on this platform";
            constexpr static c_str g_graphics_not_initialized = "Graphics is not initialized";
            constexpr static c_str g_graphics_not_suitable_device = "Not found a suitable graphics card device";
            constexpr static c_str g_graphics_unknow_adapter = "Unknow adapter. It seems that you choose a unknow adapter.";
            constexpr static c_str g_graphics_opengl_doesnt_support_swapchain = "OpenGL doesn't support SwapChain";
        }
    }
}