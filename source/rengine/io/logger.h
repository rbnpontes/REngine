#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

namespace rengine {
	namespace io {
		class ILogger {
		public:
			virtual void onLogInfo(c_str tag, c_str msg) = 0;
			virtual void onLogWarn(c_str tag, c_str msg) = 0;
			virtual void onLogError(c_str tag, c_str msg) = 0;
		};

		R_EXPORT void logger_info(c_str tag, c_str msg);
		R_EXPORT void logger_warn(c_str tag, c_str msg);
		R_EXPORT void logger_error(c_str tag, c_str msg);
		R_EXPORT void logger_set(ILogger* logger);
		R_EXPORT ILogger* logger_get();
	}
}