#pragma once
#include "./base_private.h"
#include "./rengine.h"

#include <chrono>

namespace rengine {
	using time_point = std::chrono::steady_clock::time_point;
	
	struct engine_time {
		u64 curr_frame;
		time_point last_elapsed;
		time_point curr_elapsed;
		number_t curr_delta;
	};

	struct engine_state {
		engine_update_callback callback;
		engine_time time;
		bool begin;
		bool stop;
	};

	extern engine_state g_engine_state;

	void engine__noop();
	void engine__begin();
	void engine__end();
	void engine__stop();

	void engine__begin_timer();
	void engine__end_timer();
}