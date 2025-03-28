#include "./rengine.h"
#include "./core/window_private.h"

namespace rengine {
	void init() {
		core::window__init();
	}

	void destroy() {
		core::window__deinit();
	}

	bool update() {
		core::window_poll_events();
		return true;
	}

	void run(engine_update_callback callback) {
		while (update())
			callback();
	}
}