#include "allocator.h"
#include "../defines_private.h"
#include "../exceptions.h"
#include "../strings.h"

#if ENGINE_PROFILER
    #include "./profiler_private.h"
#endif

#include <fmt/format.h>

namespace rengine{
    namespace core {
		static constexpr size_t max_size_scratch = CORE_REQUIRED_SIZE_SCRATCH_BUFFER();

        struct alloc_data {
            alloc_malloc_callback malloc;
            alloc_free_callback free;
            alloc_realloc_callback realloc;

            size_t limit;
            size_t usage;
            size_t scratch_usage;

            size_t num_alloc;
            size_t num_realloc;
            size_t num_free;
        };
        
        extern alloc_data g_data = {
            malloc,
            free,
            realloc,
            CORE_ALLOC_DEFAULT_LIMIT,
            0,
            0,
        };
		extern byte* g_scratch_buffer = null;

        void alloc__assert_limit(size_t limit) {
            if(g_data.usage + limit <= g_data.limit )
                return;
            throw alloc_exception(strings::exceptions::g_core_alloc_memory_exceeded);
        }

        ptr alloc(size_t size) {
            alloc__assert_limit(size);
            g_data.usage += size;
            ++g_data.num_alloc;

            ptr ptr = g_data.malloc(size + sizeof(size_t));
            *(size_t*)ptr = size;

#if ENGINE_PROFILER
            profiler__alloc(ptr, size);
#endif
            return (char*)ptr + sizeof(size_t);
        }

        ptr alloc_scratch(size_t size)
        {
            if(!g_scratch_buffer)
				g_scratch_buffer = (byte*)alloc(max_size_scratch);
            
            auto result = g_scratch_buffer + g_data.scratch_usage;
			auto new_size = g_data.scratch_usage + size;
            if (new_size > max_size_scratch)
                throw alloc_exception(
					fmt::format(strings::exceptions::g_core_alloc_scratch_memory_exceeded,
						g_data.scratch_usage, new_size).c_str()
                );

			g_data.scratch_usage = new_size;
            return result;
        }

        void alloc_free(ptr _ptr) {
            if(!_ptr)
                return;

            _ptr = (char*)_ptr - sizeof(size_t);
            size_t size = *(size_t*)_ptr;

#if ENGINE_PROFILER
            profiler__free(_ptr);
#endif

            g_data.usage -= size;
            g_data.free(_ptr);
            ++g_data.num_free;
        }

		void alloc_scratch_pop(size_t size) {
			if (!g_scratch_buffer)
				return;

			size_t new_size = g_data.scratch_usage - size;
            // if new_size is greater than the actual size its means
			// that we overflow the unsigned int, in this case we put it to 0
            if (new_size > g_data.scratch_usage)
                new_size = 0;
            g_data.scratch_usage = new_size;
		}

        ptr alloc_realloc(ptr _ptr, size_t size) {
            alloc__assert_limit(size);
            if(!_ptr)
                return null;
            
            _ptr = (char*)_ptr - sizeof(size_t);
            size_t old_size = *(size_t*)_ptr;
            g_data.usage -= old_size;
            ++g_data.num_realloc;

#if ENGINE_PROFILER
            profiler__free(_ptr);
#endif
            _ptr = g_data.realloc(_ptr, size);
            g_data.usage += size;

#if ENGINE_PROFILER
            profiler__alloc(_ptr, size);
#endif

            return (char*)_ptr + sizeof(size_t);
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

        size_t alloc_get_scratch_usage()
        {
            return g_data.scratch_usage;
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
