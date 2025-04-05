#pragma once
#include "../base_private.h"
#include "./logger.h"

namespace rengine {
	namespace io {
		struct logger_state {
			ILogger* current_logger{ null };
			queue<ILog*> logs;
			u32 num_logs;
		};
		extern logger_state g_logger_state;

		void logger__init();
		void logger__deinit();
		void logger__assert_logger();
		void logger__change_default_logger(ILogger* logger);
		ILog* logger__alloc_log(const string& tag);
	}
}