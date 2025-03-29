#include "./rengine.h"
#include "./core/window_private.h"
#include "./graphics/graphics_private.h"

namespace rengine {
	void init() {
		engine_init_desc desc{};
		init_ex(desc);
	}

	void init_ex(const engine_init_desc& desc) {
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

	bool update() {
		core::window_poll_events();
		return true;
	}

	void run(engine_update_callback callback) {
		while (update())
			callback();
	}
}