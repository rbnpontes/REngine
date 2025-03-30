#include "./logger.h"
#include "./logger_private.h"

#include "../exceptions.h"

namespace rengine {
	namespace io {

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

		void logger_fatal(c_str tag, c_str msg) {
			logger__assert_logger();
			g_logger->onLogFatal(tag, msg);
		}
		
		void logger_set(ILogger* logger) {
			g_logger = logger;
		}

		ILogger* logger_get() {
			return g_logger;
		}
	}
}