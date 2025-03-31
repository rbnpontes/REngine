#pragma once
#include <rengine/events/event.h>
#include <rengine/types.h>

namespace rengine {
	namespace events {
		struct window_event_args {
			ptr sdl_event;
			core::window_t window_id;
			bool skip;
		};
		typedef void (*window_sdl_event_fn)(window_event_args&);
		typedef void (*window_default_event_fn)(const core::window_t&, ptr);

		// Event Settings:  System | Name | Type			  || Emit Args
		ENGINE_EVENT_DEFINE(window, event, window_sdl_event_fn)(window_event_args& args);
		ENGINE_EVENT_DEFINE(window, quit, window_default_event_fn)(const core::window_t& window_id, ptr sdl_window);
		ENGINE_EVENT_DEFINE(window, resize, window_default_event_fn)(const core::window_t& window_id, ptr sdl_window);
	}
}