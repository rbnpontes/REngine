#pragma once
#include <rengine/types.h>

namespace rengine {
    namespace strings {
        static c_str g_backend_strings[] = {
            "d3d11",
            "d3d12",
            "vulkan",
            "webgpu",
            "opengl",
            0
        };

        namespace logs {
            static c_str g_graphics_tag = "graphics";
            static c_str g_graphics_invalid_adapter_id = "Invalid adapter id %u. Engine will try to select a best match device";
            static c_str g_graphics_no_suitable_device_found = "No suitable device found, using first available.";
        }

        namespace exceptions {
            static c_str g_graphics_not_suitable_device = "Not found a suitable graphics card device";
            static c_str g_graphics_unknow_adapter = "Unknow adapter. It seems that you choose a unknow adapter.";
        }
    }
}