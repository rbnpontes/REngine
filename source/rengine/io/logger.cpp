#include "./logger.h"
#include "../exceptions.h"
#include "./logger_iostream_impl_private.h"

namespace rengine {
	namespace io {
		static ILogger* g_default_logger = new IOStreamLogger();
		static ILogger* g_logger = g_default_logger;

		void logger__assert_logger() {
			if (!g_logger)
				throw null_exception("Logger is null");
		}

		void logger_info(c_str tag, c_str msg) {
			logger__assert_logger();
			g_logger->onLogInfo(tag, msg);
		}
	
		void logger_warn(c_str tag, c_str msg) {
			logger__assert_logger();
			g_logger->onLogWarn(tag, msg);
		}
		
		void logger_error(c_str tag, c_str msg) {
			logger__assert_logger();
			g_logger->onLogError(tag, msg);
		}
		
		void logger_set(ILogger* logger) {
			g_logger = logger;
		}

		ILogger* logger_get() {
			return g_logger;
		}
	}
}