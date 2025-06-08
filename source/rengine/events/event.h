#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <rengine/core/allocator.h>

#define __EVENT_SUBSCRIBE_SIGNATURE(system, event_name, event_fn_type) \
	system##_subscribe_##event_name##(event_fn_type evt_callback)
#define __EVENT_UNSUBSCRIBE_SIGNATURE(system, event_name, event_fn_type) \
	system##_unsubscribe_##event_name##(event_fn_type evt_callback)
#define __EVENT_EMIT_SIGNATURE(system, event_name) \
	system##_emit_##event_name

#define __EVENT_LIST_PROP(system, event_name) \
	g_##system##_##event_name##_events
#define __EVENT_LIST_COUNT_PROP(system, event_name) \
	g_##system##_##event_name##_events_count

#define __EVENT_DEFINE_METHODS(system, event_name, event_fn_type, modifier) \
	modifier __EVENT_SUBSCRIBE_SIGNATURE(system, event_name, event_fn_type); \
	modifier __EVENT_UNSUBSCRIBE_SIGNATURE(system, event_name, event_fn_type); \
	modifier __EVENT_EMIT_SIGNATURE(system, event_name)

#define __EVENT_BODY_DEFINE_PROPS(system, event_name, event_fn_type) \
	extern event_fn_type* __EVENT_LIST_PROP(system, event_name) = null; \
	extern size_t __EVENT_LIST_COUNT_PROP(system, event_name) = 0

#define __EVENT_BODY_DEFINE_SUBSCRIBE(system, event_name, event_fn_type) \
	void __EVENT_SUBSCRIBE_SIGNATURE(system, event_name, event_fn_type) \
	{ \
		__EVENT_LIST_PROP(system, event_name) = rengine::events::event_insert_item(__EVENT_LIST_PROP(system, event_name), \
			&__EVENT_LIST_COUNT_PROP(system, event_name), \
			evt_callback); \
	}
#define __EVENT_BODY_DEFINE_UNSUBSCRIBE(system, event_name, event_fn_type) \
	void __EVENT_UNSUBSCRIBE_SIGNATURE(system, event_name, event_fn_type) \
	{ \
		__EVENT_LIST_PROP(system, event_name) = rengine::events::event_remove_item(__EVENT_LIST_PROP(system, event_name), \
			&__EVENT_LIST_COUNT_PROP(system, event_name), \
			evt_callback); \
	}
#define __EVENT_BODY_DEFINE_EMIT(system, event_name) \
	void __EVENT_EMIT_SIGNATURE(system, event_name)


#define ENGINE_EVENT_DEFINE(system, event_name, event_fn_type) \
	__EVENT_DEFINE_METHODS(system, event_name, event_fn_type, R_EXPORT void)
#define EVENT_DEFINE(system, event_name, event_fn_type) \
	__EVENT_DEFINE_METHODS(system, event_name, event_fn_type, void)
#define EVENT_BODY_DEFINE(system, event_name, event_fn_type) \
	__EVENT_BODY_DEFINE_PROPS(system, event_name, event_fn_type); \
	__EVENT_BODY_DEFINE_SUBSCRIBE(system, event_name, event_fn_type) \
	__EVENT_BODY_DEFINE_UNSUBSCRIBE(system, event_name, event_fn_type) \
	__EVENT_BODY_DEFINE_EMIT(system, event_name)
#define EVENT_EMIT_BEGIN(system, event_name) \
	for (size_t event_idx = 0; event_idx < __EVENT_LIST_COUNT_PROP(system, event_name); ++event_idx) \
	{ \
		const auto event = __EVENT_LIST_PROP(system, event_name)[event_idx];
#define EVENT_EMIT_END() \
	}
#define EVENT_EMIT(system, event_name) \
	rengine::events::##system##_emit_##event_name

namespace rengine {
	namespace events {
		template<typename T>
		T* event_insert_item(T* event_list, size_t* event_count, T event_callback) {
			size_t new_count = *event_count + 1;
			T* new_event_list = null;

			if (*event_count == 0)
				new_event_list = core::alloc_array_alloc<T>(new_count);
			else {
				new_event_list = core::alloc_array_realloc(event_list, new_count);
				memcpy(new_event_list, event_list, sizeof(T) * *event_count);
			}

			new_event_list[*event_count] = event_callback;
			*event_count = new_count;
			return new_event_list;
		}

		template<typename T>
		T* event_remove_item(T* event_list, size_t* event_count, T event_callback) {
			if (*event_count == 0)
				return event_list;

			size_t event_idx = -1;
			for (size_t i = 0; i < *event_count; ++i) {
				if (event_list[i] != event_callback)
					continue;
				event_idx = i;
				break;
			}

			if (event_idx == -1)
				return event_list;

			size_t new_count = *event_count - 1;
			size_t new_mem_size = sizeof(T) * new_count;

			if (new_count == 0) {
				core::alloc_free(event_list);
				*event_count = 0;
				return null;
			}

			T* new_event_list = core::alloc_array_realloc<T>(event_list, new_count);
			size_t second_idx = 0;
			for (size_t i = 0; i < *event_count && second_idx < new_count; ++i) {
				if (i == event_idx)
					continue;
				new_event_list[second_idx] = event_list[i];
				++second_idx;
			}

			*event_count = new_count;
			return event_list;
		}
	}
}