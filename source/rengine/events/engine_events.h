#pragma once
#include <rengine/events/event.h>
#include <rengine/types.h>

namespace rengine {
	namespace events {
		typedef void(*engine_default_event_fn)();
		typedef void(*engine_update_event_fn)(number_t);
		typedef void(*engine_before_stop_event_fn)(bool*);

		// Event Settings: System | Name  | Type				  || Emit Args
		ENGINE_EVENT_DEFINE(engine, update, engine_update_event_fn)(number_t);
		ENGINE_EVENT_DEFINE(engine, begin_update, engine_default_event_fn)();
		ENGINE_EVENT_DEFINE(engine, end_update, engine_default_event_fn)();
		ENGINE_EVENT_DEFINE(engine, before_stop, engine_before_stop_event_fn)(bool* can_stop);
		ENGINE_EVENT_DEFINE(engine, stop, engine_default_event_fn)();
		ENGINE_EVENT_DEFINE(engine, destroy, engine_default_event_fn)();
	}
}