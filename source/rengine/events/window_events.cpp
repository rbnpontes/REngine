#include "./window_events.h"

namespace rengine {
	namespace events {
		EVENT_BODY_DEFINE(window, event, window_sdl_event_fn)(window_event_args& args) {
			EVENT_EMIT_BEGIN(window, event)
				event(args);
			EVENT_EMIT_END()
		}

		EVENT_BODY_DEFINE(window, quit, window_default_event_fn)(const core::window_t& window_id, ptr sdl_window) {
			EVENT_EMIT_BEGIN(window, quit)
				event(window_id, sdl_window);
			EVENT_EMIT_END()
		}

		EVENT_BODY_DEFINE(window, resize, window_default_event_fn)(const core::window_t& window_id, ptr sdl_window) {
			EVENT_EMIT_BEGIN(window, resize)
				event(window_id, sdl_window);
			EVENT_EMIT_END()
		}
	}
}