#include "./logger.h"
#include "./logger_private.h"

#include "../exceptions.h"

namespace rengine {
	namespace io {

		void logger_info(c_str tag, c_str msg) {
			logger__assert_logger();
			g_logger_state.current_logger->onLogInfo(tag, msg);
		}
	
		void logger_warn(c_str tag, c_str msg) {
			logger__assert_logger();
			g_logger_state.current_logger->onLogWarn(tag, msg);
		}
		
		void logger_error(c_str tag, c_str msg) {
			logger__assert_logger();
			g_logger_state.current_logger->onLogError(tag, msg);
		}

		void logger_fatal(c_str tag, c_str msg) {
			logger__assert_logger();
			g_logger_state.current_logger->onLogFatal(tag, msg);
		}

		ILog* logger_use(c_str tag) {
			logger__assert_logger();
			return logger__alloc_log(tag);
		}
		
		void logger_set(ILogger* logger) {
			logger__change_default_logger(logger);
		}

		ILogger* logger_get() {
			return g_logger_state.current_logger;
		}
	}
}