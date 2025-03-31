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
		io::logger__init();
		core::window__init();
		graphics::init({
			desc.window_id,
			desc.adapter_id,
			desc.backend,
		});
	}

	void destroy() {
		core::window__deinit();
	}

	number_t get_delta_time() {
		return g_engine_state.time.curr_delta;
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