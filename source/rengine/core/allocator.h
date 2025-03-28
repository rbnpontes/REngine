#pragma once
#include <rengine/api.h>
#include <utility>
#include <new>

namespace rengine {
    namespace core {
        typedef void* (*alloc_malloc_callback_)(size_t);
        typedef void (*alloc_free_callback_)(void*);
        typedef void* (*alloc_realloc_callback_)(void*, size_t);
        typedef alloc_malloc_callback_ alloc_malloc_callback;
        typedef alloc_free_callback_ alloc_free_callback;
        typedef alloc_realloc_callback_ alloc_realloc_callback;

        R_EXPORT void* alloc(size_t size);
        R_EXPORT void alloc_free(void* ptr);
        R_EXPORT void* alloc_realloc(void* ptr, size_t size);
        R_EXPORT void alloc_set_limit(size_t limit);
        R_EXPORT size_t alloc_get_limit();
        R_EXPORT size_t alloc_get_usage();
        R_EXPORT void alloc_set_malloc_callback(const alloc_malloc_callback callback);
        R_EXPORT void alloc_set_free_callback(const alloc_free_callback callback);
        R_EXPORT void alloc_set_realloc_callback(const alloc_realloc_callback callback);

        template <typename T, typename... Args>
        inline T* alloc_new(Args&&... args) {
            T* ptr = (T*)alloc(sizeof(T));
            return new(ptr) T(std::forward<Args>(args)...);
        }
    }
}

void* operator new[](size_t size, const char* pName, int flags, unsigned debugFlags, const char* file, int line);
void* operator new[](size_t size, size_t alignment, size_t alignmentOffset, const char* pName, int flags, unsigned debugFlags, const char* file, int line);