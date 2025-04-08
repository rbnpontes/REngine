#pragma once
#include <rengine/strings.h>
#include <rengine/exceptions.h>
#include <rengine/core/core.h>
#include <rengine/graphics/graphics.h>
#include <rengine/math/math.h>
#include <rengine/io/io.h>
#include <rengine/events/events.h>

namespace rengine {
	typedef void (*__engine_update_call)();
	typedef __engine_update_call engine_update_callback;

	struct engine_init_desc {
		core::window_t		window_id	{ core::no_window };
		u8					adapter_id	{ MAX_U8_VALUE };
		graphics::backend	backend		{ GRAPHICS_BACKEND_DEFAULT };
	};

	R_EXPORT void init();
	R_EXPORT void init_ex(const engine_init_desc& desc);
	R_EXPORT void update();
	R_EXPORT void run(engine_update_callback callback);
	R_EXPORT void stop();
	R_EXPORT void destroy();
	R_EXPORT number_t get_delta_time();
	R_EXPORT void use_window(const core::window_t& window_id);
	R_EXPORT const core::window_t& get_window();

	R_EXPORT void begin();
	R_EXPORT void end();
}