#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <rengine/core/allocator.h>
PRIVATE_HEADER

namespace rengine {
	namespace core {
		class EngineStlAllocator {
		public:
			EngineStlAllocator(c_str name = "rengine") : name_(name) {}
			EngineStlAllocator(const EngineStlAllocator& x) {
				name_ = x.name_;
			}
			EngineStlAllocator(const EngineStlAllocator& x, c_str name): name_(name) {
			}

			void* allocate(size_t size, int flags = 0) {
				return alloc(size);
			}
			void* allocate(size_t n, size_t alignment, size_t offset, int flags) {
				return alloc(n);
			}
			void deallocate(void* ptr, size_t size) {
				alloc_free(ptr);
			}

			c_str get_name() const { return name_; }
			void set_name(c_str name) { name_ = name; }

			bool operator==(const EngineStlAllocator& target) const {
				return strcmp(name_, target.name_) == 0;
			}
			bool operator!=(const EngineStlAllocator& target) const {
				return strcmp(name_, target.name_) != 0;
			}
		private:
			c_str name_;
		};
	}
}

#define EASTLAllocatorType EngineStlAllocator