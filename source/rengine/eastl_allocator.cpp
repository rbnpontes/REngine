#include "eastl_allocator.h"

namespace rengine {
    extern EngineEASTLAllocator* g_eastl_allocator = null;

    EngineEASTLAllocator* get_eastl_allocator() {
        if (!g_eastl_allocator)
            g_eastl_allocator = new EngineEASTLAllocator(EASTL_ALLOCATOR_DEFAULT_NAME);
        return g_eastl_allocator;
    }
}