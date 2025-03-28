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
	namespace core {
		ENGINE_DEFINE_EXCEPTION(alloc_exception);
		ENGINE_DEFINE_EXCEPTION(window_exception);
	}
}