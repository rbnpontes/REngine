#pragma once
#include <rengine/strings.h>
#include <rengine/exceptions.h>
#include <rengine/core/core.h>
#include <rengine/graphics/graphics.h>
#include <rengine/math/math-types.h>

namespace rengine {
	typedef void (*__engine_update_call)();
	typedef __engine_update_call engine_update_callback;

	struct engine_init_desc {
		core::window_t		window_id	{ core::no_window };
		graphics::backend	backend		{ GRAPHICS_BACKEND_DEFAULT };
	};

	R_EXPORT void init();
	R_EXPORT void init_ex(const engine_init_desc& desc);
	R_EXPORT bool update();
	R_EXPORT void run(engine_update_callback callback);
	R_EXPORT void destroy();
}