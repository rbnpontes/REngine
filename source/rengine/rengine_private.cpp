#include "./rengine_private.h"
#include "./events/engine_events.h"
#include "./graphics/graphics_private.h"

#include <chrono>


namespace rengine {
	engine_state g_engine_state = {};

	void engine__noop()
	{
	}

	void engine__begin() {
		if (g_engine_state.begin)
			return;

		g_engine_state.begin = true;
		
		EVENT_EMIT(engine, begin_update)();

		engine__begin_timer();
		graphics::begin();

		EVENT_EMIT(engine, update)(g_engine_state.time.curr_delta);
	}

	void engine__end() {
		if (!g_engine_state.begin)
			return;
		g_engine_state.begin = false;

		engine__end_timer();
		graphics::end();

		EVENT_EMIT(engine, end_update)();
	}

	void engine__stop()
	{
		if (g_engine_state.stop) {
			io::logger_warn(strings::logs::g_engine_tag, strings::logs::g_engine_already_stopped);
			return;
		}

		bool can_stop = true;
		EVENT_EMIT(engine, before_stop)(&can_stop);

		if (!can_stop)
			return;

		EVENT_EMIT(engine, stop)();
		g_engine_state.stop = true;
	}

	void engine__begin_timer() {
		auto& time = g_engine_state.time;

		time.last_elapsed = time.curr_elapsed;
		time.curr_elapsed = std::chrono::high_resolution_clock::now();
		++time.curr_frame;
	}

	void engine__end_timer() {
		auto& time = g_engine_state.time;
		auto delta = time.curr_elapsed - time.last_elapsed;
		time.curr_delta = (number_t)delta.count();
	}
}