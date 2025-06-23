#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <exception>

#define ENGINE_DEFINE_EXCEPTION(exception_name) \
	class R_EXPORT exception_name : public std::exception { \
	public: \
		exception_name(c_str message) : std::exception(message){} \
	}

namespace rengine {
	class R_EXPORT not_implemented_exception : public std::exception {
	public:
		not_implemented_exception() : std::exception("Not implemented"){}
	};

	ENGINE_DEFINE_EXCEPTION(null_exception);
	ENGINE_DEFINE_EXCEPTION(engine_exception);
	namespace core {
		ENGINE_DEFINE_EXCEPTION(alloc_exception);
		ENGINE_DEFINE_EXCEPTION(window_exception);
		ENGINE_DEFINE_EXCEPTION(pool_exception);
		ENGINE_DEFINE_EXCEPTION(profiler_exception);
	}

	namespace io {
		ENGINE_DEFINE_EXCEPTION(logger_exception);
	}

        namespace graphics {
                ENGINE_DEFINE_EXCEPTION(graphics_exception);
        }
        namespace resources {
                ENGINE_DEFINE_EXCEPTION(resource_exception);
        }
}