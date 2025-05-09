#pragma once
#include "../base_private.h"
#include "./profiler.h"

#if ENGINE_PROFILER
	#include <tracy/Tracy.hpp>
	#include <tracy/TracyC.h>
#endif

namespace rengine {
	namespace core {
		enum class profiler_mem_op {
			alloc = 0,
			free = 1
		};
		typedef void (*profiler_alloc_fn)(ptr mem, size_t size);
		typedef void (*profiler_free_fn)(ptr mem);

#if ENGINE_PROFILER
		void profiler__alloc_direct(ptr mem, size_t size);
		void profiler__free_direct(ptr mem);
		void profiler__alloc_delayed(ptr mem, size_t size);
		void profiler__free_delayed(ptr mem);
		void profiler__alloc_stub(ptr mem, size_t size);
		void profiler__free_stub(ptr mem);
#endif

#if ENGINE_PROFILER
		struct profiler_zone_entry {
			u32 id;
			u32 prev_idx;
		};

		struct profiler_mem_alloc_info {
			ptr mem;
			size_t size;
			profiler_mem_op op;
			profiler_mem_alloc_info* next;
		};

		struct profiler_mem_alloc_link {
			profiler_mem_alloc_info* begin{ null };
			profiler_mem_alloc_info* end{ null };
		};
		struct profiler_state {
			fixed_vector<profiler_entry_info, CORE_MAX_PROFILER_ENTRIES> entries;
			fixed_vector<profiler_zone_entry, CORE_MAX_PROFILER_ENTRIES> zones;
			u32 curr_zone { MAX_U32_VALUE };
			profiler_mem_alloc_link delayed_mem_alloc;
			profiler_alloc_fn malloc_call{ profiler__alloc_delayed };
			profiler_free_fn free_call{ profiler__free_delayed };
			bool connected{ false };
		};
		extern profiler_state g_profiler_state;
#endif

		void profiler__init();
		void profiler__deinit();

		bool profiler__enabled();
		bool profiler__started();

		void profiler__alloc(ptr mem, size_t size);
		void profiler__free(ptr mem);


		void profiler__begin_frame();
		void profiler__end_frame();
		void profiler__entry_push(profiler_entry_info* entry);
		void profiler__entry_pop();
		void profiler__log(c_str str);
	}
}