#include "./engine_events.h"

namespace rengine {
	namespace events {
		EVENT_BODY_DEFINE(engine, update, engine_update_event_fn)(number_t delta_time) 
		{
			EVENT_EMIT_BEGIN(engine, update)
				event(delta_time);
			EVENT_EMIT_END()
		}

		EVENT_BODY_DEFINE(engine, begin_update, engine_default_event_fn)()
		{
			EVENT_EMIT_BEGIN(engine, begin_update)
				event();
			EVENT_EMIT_END()
		}

		EVENT_BODY_DEFINE(engine, end_update, engine_default_event_fn)()
		{
			EVENT_EMIT_BEGIN(engine, end_update)
				event();
			EVENT_EMIT_END()
		}
	}
}