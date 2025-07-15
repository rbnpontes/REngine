#include "./arena_private.h"
#include "./hash.h"

namespace rengine {
	namespace core {
		arena_state g_arena_state = {};

		void arena__init() {
			g_arena_state.log = io::logger_use(strings::logs::g_arena_tag);
			g_arena_state.default_arena = arena_create_default();
			g_arena_state.scratch_arena = arena_create_scratch(CORE_REQUIRED_SIZE_SCRATCH_BUFFER());
		}

		void arena__deinit() {
			auto link = g_arena_state.root;
			while (link) {
				auto prev = link->prev;
				auto* arena = arena__get_arena_from_link(link);

				link = prev;
				arena__destroy(arena);
			}

			g_arena_state.default_arena = null;
			g_arena_state.root = null;
			g_arena_state.count = 0;
		}

		void arena__push(IArena* arena)
		{
			auto curr = g_arena_state.root;
			auto header = arena__get_header(arena);

			auto* link = &header->curr_link;
			link->prev = curr;
			if (null != curr)
				curr->next = link;
			g_arena_state.root = link;
		}

		arena_header_t* arena__get_header(IArena* arena)
		{
			byte* data = (byte*)arena;
			data -= sizeof(arena_header_t);

			auto* header = (arena_header_t*)data;
			if (header->curr_link.kind == arena_kind::unknown)
				return null;

			// Simple integration validation
			const auto hash = arena__hash_header(header);
			return hash == header->hash ? header : null;
		}

		IArena* arena__get_arena_from_link(arena_link_t* link)
		{
			byte* data = (byte*)link;
			data += sizeof(arena_header_t);
			return (IArena*)data;
		}

		core::hash_t arena__hash_header(arena_header_t* header)
		{
			return core::hash((ptr)header);
		}

		core::hash_t arena__hash_link(const arena_link_t& link)
		{
			const byte* data = reinterpret_cast<const byte*>(&link);
			return hash(data, sizeof(arena_link_t));
		}

		void arena__destroy(IArena* arena)
		{
			auto* header = arena__get_header(arena);
			if (!header) {
				g_arena_state.log->error(strings::logs::g_arena_free_invalid);
				return;
			}
			
			auto& link = header->curr_link;
			auto prev = link.prev;
			auto next = link.next;

			if (prev)
				prev->next = next;
			if (next)
				next->prev = prev;

			link.prev = link.next = null;
			arena->~IArena();
			core::alloc_free(header);
		}
	}
}