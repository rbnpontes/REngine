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
		io::logger__init();
		core::window__init();
		graphics::init({
			desc.window_id,
			desc.adapter_id,
			desc.backend,
		});

		if (desc.window_id != core::no_window)
			use_window(desc.window_id);
	}

	void destroy() {
		core::window__deinit();
	}

	number_t get_delta_time() {
		return g_engine_state.time.curr_delta;
	}

	void use_window(const core::window_t& window_id)
	{

	}

	bool update() {
		core::window_poll_events();
		return true;
	}

	void run(engine_update_callback callback) {
		while (update())
		{
			begin();
			callback();
			end();
		}
	}

	void begin() {
		engine__begin();
	}

	void end() {
		engine__end();
	}
}