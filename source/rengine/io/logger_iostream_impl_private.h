#pragma once
#include "../base_private.h"
#include "./logger.h"

namespace rengine {
	namespace io {
		enum class log_type {
			info = 0,
			warn,
			err,
			fatal
		};

		constexpr static c_str g_log_type_entries[] = {
			"info",
			"warn",
			"error",
			"fatal"
		};

		class IOStreamLogger : public ILogger {
		public:
			IOStreamLogger() {}
			void onLogInfo(c_str tag, c_str msg) override;
			void onLogWarn(c_str tag, c_str msg) override;
			void onLogError(c_str tag, c_str msg) override;
			void onLogFatal(c_str tag, c_str msg) override;
		private:
			void onLog(log_type type, c_str tag, c_str msg);
		};
	}
}