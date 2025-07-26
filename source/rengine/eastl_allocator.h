#pragma once
#define EASTL_ALLOCATOR_DEFAULT_NAME "REngine"
#include <rengine/core/allocator.h>

namespace rengine {
    class EngineEASTLAllocator {
    public:
        explicit EngineEASTLAllocator(c_str name = nameof(EngineEASTLAllocator)): name_(name) {}
        EngineEASTLAllocator(const EngineEASTLAllocator& allocator) { name_ = allocator.name_; }
        EngineEASTLAllocator(const EngineEASTLAllocator& allocator, c_str name): name_(name) {}

        EngineEASTLAllocator& operator=(const EngineEASTLAllocator& allocator) { name_ = allocator.name_; return *this; }

        ptr allocate(size_t size, int flags = 0) { return core::alloc(size); }
        ptr allocate(size_t size, size_t alignment, size_t offset, int flags = 0) { return core::alloc(size); }
        void deallocate(ptr p, size_t size = 0) { core::alloc_free(p); }

        c_str get_name() const { return name_; }
        void set_name(c_str name) { name_ = name; }
    private:
        c_str name_;
    };

    inline bool operator==(const EngineEASTLAllocator& lhs, const EngineEASTLAllocator& rhs) {
        return true;
    }
    inline bool operator!=(const EngineEASTLAllocator& lhs, const EngineEASTLAllocator& rhs) {
        return false;
    }

    R_EXPORT EngineEASTLAllocator* get_eastl_allocator();
}