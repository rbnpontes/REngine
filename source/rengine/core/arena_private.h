#pragma once
#include "../base_private.h"
#include "./arena.h"

#include "../io/logger.h"

namespace rengine {
	namespace core {
		enum class arena_kind : u32 {
			normal = 0,
			frame,
			fixed,
			scratch,
			unknown
		};

		struct arena_link_t {
			arena_link_t* prev{ null };
			arena_link_t* next{ null };
			arena_kind kind{ arena_kind::unknown };
		};

		struct arena_header_t {
			arena_link_t curr_link {};
			core::hash_t hash{ 0 };
		};

		struct arena_state {
			arena_link_t* root { null };
			IArena* default_arena{ null };
			size_t count{ 0 };
			io::ILog* log{ null };
		};
		extern arena_state g_arena_state;

		void arena__init();
		void arena__deinit();

		void arena__push(IArena* arena);

		void arena__destroy(IArena* arena);
		arena_header_t* arena__get_header(IArena* arena);
		IArena* arena__get_arena_from_link(arena_link_t* link);

		core::hash_t arena__hash_header(arena_header_t* header);
		core::hash_t arena__hash_link(const arena_link_t& link);

		template <class T>
		inline T* arena__alloc(arena_kind kind) {
			byte* data = core::alloc(arena_size + sizeof(arena_header_t));
			arena_header_t* header = (arena_header_t*)data;
			*header = arena_header_t{};
			header->curr_link.kind = kind;
			header->hash = arena__hash_header(header);
			
			T* arena = data + sizeof(arena_header_t);
			return new(arena) T();
		}

		template <class T>
		constexpr T* arena__get_arena(arena_header_t* header) noexcept {
			byte* data = (byte*)header;
			data += sizeof(arena_header_t);
			return reinterpret_cast<T*>(data);
		}
	}
}