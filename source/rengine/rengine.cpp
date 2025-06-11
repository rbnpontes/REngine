#include "./rengine.h"
#include "./rengine_private.h"
#include "./core/string_pool_private.h"
#include "./core/profiler.h"
#include "./core/window_private.h"
#include "./core/profiler_private.h"
#include "./graphics/graphics_private.h"
#include "./io/logger_private.h"

namespace rengine {
	void init() {
		engine_init_desc desc{};
		init_ex(desc);
	}

	void init_ex(const engine_init_desc& desc) {
		g_engine_state.callback = engine__noop;
		if (desc.window_id != core::no_window)
			use_window(desc.window_id);

		action_t actions[] = {
			core::string_pool__init,
			io::logger__init,
			core::window__init,
			core::profiler__init,
		};

		for(u32 i = 0; i < _countof(actions); ++i)
			actions[i]();

		graphics::init({
			desc.window_id,
			desc.adapter_id,
			desc.backend,
		});
	}

	void destroy() {
		EVENT_EMIT(engine, destroy)();

		action_t actions[] = {
			graphics::deinit,
			core::window__deinit,
			io::logger__deinit,
			core::profiler__deinit,
			core::string_pool__deinit,
		};

		for (u32 i = 0; i < _countof(actions); ++i)
			actions[i]();
	}

	number_t get_delta_time() {
		return g_engine_state.time.curr_delta;
	}

	void use_window(const core::window_t& window_id)
	{
		g_engine_state.window_id = window_id;
	}

	const core::window_t& get_window()
	{
		return g_engine_state.window_id;
	}

	void enable_fps_monitor()
	{
		g_engine_state.monitor.fps = true;
	}

	void hide_fps_monitor()
	{
		g_engine_state.monitor.fps = false;
	}

	void update() {
		if (g_engine_state.stop)
			return;

		engine__begin();
		{
			profile_name(nameof(game_update));
			g_engine_state.callback();
		}
		engine__end();
	}

	void run(engine_update_callback callback) {
		g_engine_state.callback = callback;
		while (!g_engine_state.stop)
			update();
	}

	void stop()
	{
		engine__stop();
	}
}