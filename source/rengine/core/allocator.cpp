#include "allocator.h"
#include "../defines.h"
#include "../exceptions.h"

#if ENGINE_PROFILER
    #include "./profiler_private.h"
#endif


namespace rengine{
    namespace core {
        struct alloc_data {
            alloc_malloc_callback malloc;
            alloc_free_callback free;
            alloc_realloc_callback realloc;

            size_t limit;
            size_t usage;

            size_t num_alloc;
            size_t num_realloc;
            size_t num_free;
        };
        
        extern alloc_data g_data = {
            malloc,
            free,
            realloc,
            CORE_ALLOC_DEFAULT_LIMIT,
            0
        };

        void alloc__assert_limit(size_t limit) {
            if(g_data.usage + limit <= g_data.limit )
                return;
            throw alloc_exception("Memory limit exceeded!");
        }

        void* alloc(size_t size) {
            alloc__assert_limit(size);
            g_data.usage += size;
            ++g_data.num_alloc;

            void* ptr = g_data.malloc(size + sizeof(size_t));
            *(size_t*)ptr = size;

#if ENGINE_PROFILER
            profiler__alloc(ptr, size);
#endif
            return (char*)ptr + sizeof(size_t);
        }

        void alloc_free(void* ptr) {
            if(!ptr)
                return;

            ptr = (char*)ptr - sizeof(size_t);
            size_t size = *(size_t*)ptr;

#if ENGINE_PROFILER
            profiler__free(ptr);
#endif

            g_data.usage -= size;
            g_data.free(ptr);
            ++g_data.num_free;
        }

        void* alloc_realloc(void* ptr, size_t size) {
            alloc__assert_limit(size);
            if(!ptr)
                return null;
            
            ptr = (char*)ptr - sizeof(size_t);
            size_t old_size = *(size_t*)ptr;
            g_data.usage -= old_size;
            ++g_data.num_realloc;

#if ENGINE_PROFILER
            profiler__free(ptr);
#endif
            ptr = g_data.realloc(ptr, size);
            g_data.usage += size;

#if ENGINE_PROFILER
            profiler__alloc(ptr, size);
#endif

            return (char*)ptr + sizeof(size_t);
        }

        void alloc_set_limit(size_t limit) {
            g_data.limit = limit;
        }
        size_t alloc_get_limit() {
            return g_data.limit;
        }
        size_t alloc_get_usage() {
            return g_data.usage;
        }

        void alloc_set_malloc_callback(const alloc_malloc_callback callback) {
            g_data.malloc = callback;
        }
        void alloc_set_free_callback(const alloc_free_callback callback) {
            g_data.free = callback;
        }
        void alloc_set_realloc_callback(const alloc_realloc_callback callback) {
            g_data.realloc = callback;
        }
    }
}

void* operator new[](size_t size, const char* pName, int flags, unsigned debugFlags, const char* file, int line)
{
    return rengine::core::alloc(size);
}

void* operator new[](size_t size, size_t alignment, size_t alignmentOffset, const char* pName, int flags, unsigned debugFlags, const char* file, int line)
{
    return rengine::core::alloc(size);
}
