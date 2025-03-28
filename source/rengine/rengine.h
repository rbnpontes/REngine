#pragma once
#include <rengine/exceptions.h>
#include <rengine/core/core.h>
#include <rengine/graphics/graphics.h>
#include <rengine/math/math-types.h>

namespace rengine {
	typedef void (*__engine_update_call)();
	typedef __engine_update_call engine_update_callback;

	R_EXPORT void init();
	R_EXPORT bool update();
	R_EXPORT void run(engine_update_callback callback);
	R_EXPORT void destroy();
}