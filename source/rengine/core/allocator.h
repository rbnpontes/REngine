#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <utility>
#include <new>

namespace rengine {
    namespace core {
        typedef ptr (*alloc_malloc_callback_)(size_t);
        typedef void (*alloc_free_callback_)(ptr);
        typedef ptr (*alloc_realloc_callback_)(ptr, size_t);
        typedef alloc_malloc_callback_ alloc_malloc_callback;
        typedef alloc_free_callback_ alloc_free_callback;
        typedef alloc_realloc_callback_ alloc_realloc_callback;

        R_EXPORT ptr alloc(size_t size);
        R_EXPORT void alloc_free(ptr _ptr);
        R_EXPORT ptr alloc_scratch(size_t size);
		R_EXPORT void alloc_scratch_pop(size_t size);
        R_EXPORT ptr alloc_realloc(ptr _ptr, size_t size);
        R_EXPORT void alloc_set_limit(size_t limit);
        R_EXPORT size_t alloc_get_limit();
        R_EXPORT size_t alloc_get_usage();
		R_EXPORT size_t alloc_get_scratch_usage();
        R_EXPORT size_t alloc_get_pointer_size(ptr _ptr);
        R_EXPORT void alloc_set_malloc_callback(const alloc_malloc_callback callback);
        R_EXPORT void alloc_set_free_callback(const alloc_free_callback callback);
        R_EXPORT void alloc_set_realloc_callback(const alloc_realloc_callback callback);

        template <typename T, typename... Args>
        inline T* alloc_new(Args&&... args) {
            T* ptr = (T*)alloc(sizeof(T));
            return new(ptr) T(std::forward<Args>(args)...);
        }

        template <typename T>
        inline T* alloc_array_alloc(size_t count) {
            T* ptr = (T*)alloc(sizeof(T) * count);
            return ptr;
        }

        template <typename T>
        inline T* alloc_array_realloc(T* array, size_t count) {
            T* ptr = (T*)alloc_realloc(array, sizeof(T) * count);
            return ptr;
        }
    }
}

void* operator new[](size_t size, const char* pName, int flags, unsigned debugFlags, const char* file, int line);
void* operator new[](size_t size, size_t alignment, size_t alignmentOffset, const char* pName, int flags, unsigned debugFlags, const char* file, int line);