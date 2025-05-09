#include "profiler_private.h"
#include "./allocator.h"
#include "../exceptions.h"

namespace rengine {
	namespace core {
		profiler_state g_profiler_state = {};

		void profiler__init()
		{
#if ENGINE_PROFILER
			___tracy_startup_profiler();
			TracySetProgramName(nameof(rengine));

			auto& state = g_profiler_state;
			state.malloc_call = profiler__alloc_direct;
			state.free_call = profiler__free_direct;

			// digest memory allocations
			auto mem_info = g_profiler_state.delayed_mem_alloc.begin;
			while (mem_info != null) {
				if (mem_info->op == profiler_mem_op::alloc)
					TracyAlloc(mem_info->mem, mem_info->size);
				else
					TracyFree(mem_info->mem);

				auto curr_mem_info = mem_info;
				mem_info = mem_info->next;
				free(curr_mem_info);
			}
#endif
		}

		void profiler__deinit()
		{
#if ENGINE_PROFILER
			___tracy_shutdown_profiler();

			auto& state = g_profiler_state;
			state.malloc_call = profiler__alloc_stub;
			state.free_call = profiler__free_stub;
#endif
		}

		bool profiler__enabled()
		{
#if ENGINE_PROFILER
			return true;
#else
			return false;
#endif
		}

		bool profiler__started()
		{
#if ENGINE_PROFILER
			return ___tracy_profiler_started();
#else
			return false;
#endif
		}

		void profiler__alloc(ptr mem, size_t size)
		{
#if ENGINE_PROFILER
			auto& state = g_profiler_state;
			state.malloc_call(mem, size);
#endif
		}

		void profiler__free(ptr mem)
		{
#if ENGINE_PROFILER
			auto& state = g_profiler_state;
			state.free_call(mem);
#endif
		}

#if ENGINE_PROFILER
		void profiler__alloc_direct(ptr mem, size_t size)
		{
			TracyAlloc(mem, size);
		}

		void profiler__free_direct(ptr mem)
		{
			TracyFree(mem);
		}

		void profiler__alloc_delayed(ptr mem, size_t size)
		{
			auto& delayed_mem_alloc = g_profiler_state.delayed_mem_alloc;
			
			// Allocation occurs before app start, so 'new' operator is called
			// even before allocator initialization, for this reason we will use malloc instead
			// to allocate our linked list
			profiler_mem_alloc_info* node = (profiler_mem_alloc_info*)malloc(sizeof(profiler_mem_alloc_info));
			*node = profiler_mem_alloc_info{};
			node->mem = mem;
			node->size = size;
			node->op = profiler_mem_op::alloc;

			if (delayed_mem_alloc.begin == null)
				delayed_mem_alloc.begin = node;

			if (delayed_mem_alloc.end != null)
				delayed_mem_alloc.end->next = node;
			delayed_mem_alloc.end = node;
		}

		void profiler__free_delayed(ptr mem)
		{
			profiler_mem_alloc_info* node = (profiler_mem_alloc_info*)malloc(sizeof(profiler_mem_alloc_info));
			*node = profiler_mem_alloc_info{};
			node->mem = mem;
			node->op = profiler_mem_op::free;

			auto& delayed_mem_alloc = g_profiler_state.delayed_mem_alloc;

			if (delayed_mem_alloc.begin == null)
				delayed_mem_alloc.begin = node;

			if (delayed_mem_alloc.end != null)
				delayed_mem_alloc.end->next = node;
		}

		void profiler__alloc_stub(ptr mem, size_t size)
		{
			// noop
		}

		void profiler__free_stub(ptr mem)
		{
			// noop
		}
#endif
		
		void profiler__begin_frame()
		{
#if ENGINE_PROFILER
			auto& state = g_profiler_state;
			state.connected = tracy::GetProfiler().IsConnected();
			state.zones.reset_lose_memory();
			state.entries.reset_lose_memory();
			state.curr_zone = MAX_U32_VALUE;
			FrameMarkStart(strings::profiler::engine_loop);
#endif
		}

		void profiler__end_frame()
		{
#if ENGINE_PROFILER
			FrameMarkEnd(strings::profiler::engine_loop);
#endif
		}

		void profiler__entry_push(profiler_entry_info* entry)
		{
#if ENGINE_PROFILER
			auto& state = g_profiler_state;
			// we must skip if profiler isn't connected
			if (!state.connected)
				return;

#if ENGINE_DEBUG
			if (state.zones.size() == CORE_MAX_PROFILER_ENTRIES)
				throw profiler_exception(strings::exceptions::g_profiler_reached_entries);
#endif
			// time must get before zone insertion logic
			// this will give more precise time
			u64 time = tracy::Profiler::GetTime();

			auto& zones = state.zones;
			auto& zone = zones.push_back();
			zone.id = tracy::GetProfiler().GetNextZoneId();
			zone.prev_idx = state.curr_zone;
			state.curr_zone = state.entries.size();

			state.entries.push_back(*entry);

			// commit our profile data to tracy
			{
				TracyQueuePrepareC(tracy::QueueType::ZoneValidation);
				item->zoneValidation.id = zone.id;
				TracyQueueCommitC(zoneValidationThread);
			}

			{
				TracyQueuePrepareC(tracy::QueueType::ZoneBegin);
				item->zoneBegin.time = time;
				item->zoneBegin.srcloc = (uint64_t)&state.entries.back();
				TracyQueueCommitC(zoneBeginThread);
			}
#endif
		}

		void profiler__entry_pop()
		{
#if ENGINE_PROFILER
			auto& state = g_profiler_state;
			// we must skip if profiler isn't connected
			if (!state.connected || state.curr_zone == MAX_U32_VALUE)
				return;

			auto& zone = state.zones[state.curr_zone];
			state.curr_zone = zone.prev_idx;

			// commit our profile data to tracy
			{
				TracyQueuePrepareC(tracy::QueueType::ZoneValidation);
				item->zoneValidation.id = zone.id;
				TracyQueueCommitC(zoneValidationThread);
			}

			{
				TracyQueuePrepareC(tracy::QueueType::ZoneEnd);
				item->zoneEnd.time = tracy::Profiler::GetTime();
				TracyQueueCommitC(zoneEndThread);
			}
#endif
		}

		void profiler__log(c_str str)
		{
#if ENGINE_PROFILER
			TracyMessage(str, strlen(str));
#endif
		}
	}
}
