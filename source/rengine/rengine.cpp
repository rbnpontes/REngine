#include "./rengine.h"
#include "./rengine_private.h"
#include "./core/window_private.h"
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

		io::logger__init();
		core::window__init();
		graphics::init({
			desc.window_id,
			desc.adapter_id,
			desc.backend,
		});
	}

	void destroy() {
		EVENT_EMIT(engine, destroy)();

		graphics::deinit();
		core::window__deinit();
		io::logger__deinit();
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

	void update() {
		if (g_engine_state.stop)
			return;

		core::window_poll_events();
		begin();
		g_engine_state.callback();
		end();
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

	void begin() {
		engine__begin();
	}

	void end() {
		engine__end();
	}
}