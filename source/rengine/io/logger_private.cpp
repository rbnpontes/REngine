#include "./logger_private.h"
#include "./logger_iostream_impl_private.h"
#include "../strings.h"
#include "../exceptions.h"

#include <fmt/format.h>

namespace rengine {
	namespace io {
		ILogger* g_logger = null;

		void logger__init() {
			g_logger = new IOStreamLogger();
		}

		void logger__assert_logger() {
			if (g_logger)
				return;
			throw null_exception(
				fmt::format(strings::exceptions::g_null_object, "Logger").c_str()
			);
		}
	}
}