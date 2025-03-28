#pragma once
#include "../types.h"
#include "./allocator.h"

namespace rengine {
	namespace core {
		template<size_t N>
		struct AllocatorIdentifier {
			constexpr AllocatorIdentifier(const char(&src)[N]) {
				for (size_t i = 0; i < N; ++i)
					str[i] = src[i];
			}

			c_str c_str() const {
				return str;
			}

			char str[N];
		};

		class EngineStlAllocator {
		public:
			explicit EngineStlAllocator(c_str name = "rengine") : name_(name) {}
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

		template<AllocatorIdentifier identifier>
		class EngineAllocator : public EngineStlAllocator {
		public:
			EngineAllocator() : EngineStlAllocator(identifier.c_str()){}
		};

		//EngineStlAllocator* g_default_stl_allocator_ptr = &g_default_stl_allocator;

		/*EngineStlAllocator* get_default_allocator() {
			return &g_default_stl_allocator;
		}*/
	}
}