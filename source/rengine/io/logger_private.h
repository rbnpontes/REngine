#pragma once
#include "../types.h"
#include "./logger.h"

namespace rengine {
	namespace io {
		extern rengine::io::ILogger* g_logger;

		void logger__init();
		void logger__assert_logger();
	}
}