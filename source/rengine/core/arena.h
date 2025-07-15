#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

namespace rengine {
	namespace core {
		class IArena {
		public:
			virtual ptr alloc(size_t size) = 0;
			virtual ptr realloc(ptr mem, size_t new_size) = 0;
			virtual void free(ptr mem) = 0;
			virtual size_t usage() const = 0;
			virtual size_t size() const = 0;
		};

		class IFrameArena : public IArena {
		public:
			virtual void reset() = 0;
			virtual size_t get_blocks_count() const = 0;
			virtual void destroy_block() = 0;
			virtual void destroy_all_blocks() = 0;
		};

		class IScratchArena : public IArena {
		public:
			virtual void resize(size_t scratch_size) = 0;
		};

		R_EXPORT IArena* arena_create_default();
		R_EXPORT IFrameArena* arena_create_frame(size_t initial_size);
		R_EXPORT IFrameArena* arena_create_fixed(size_t max_size);
		R_EXPORT IScratchArena* arena_create_scratch(size_t scratch_size);
		/*
		* Destroy an Arena allocated by the Engine
		* Don't use this method to destroy your own
		* implementation.
		*/
		R_EXPORT void arena_destroy(IArena* arena);

		R_EXPORT IArena* arena_get_default();
		R_EXPORT IScratchArena* arena_get_scratch();
	}
}