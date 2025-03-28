#pragma once
#include <rengine/types.h>

namespace rengine {
    namespace graphics {
        static c_str g_backend_strings[] = {
            "d3d11",
            "d3d12",
            "vulkan",
            "webgpu",
            "opengl",
            0
        };
    }
}