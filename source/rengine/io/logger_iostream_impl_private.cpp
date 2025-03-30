#include "./logger_iostream_impl_private.h"
#include "../strings.h"

#include <iostream>
#include <ctime>
#include <fmt/format.h>

namespace rengine {
	namespace io {
		void IOStreamLogger::onLogInfo(c_str tag, c_str msg) {
			onLog(log_type::info, tag, msg);
		}

		void IOStreamLogger::onLogWarn(c_str tag, c_str msg) {
			onLog(log_type::warn, tag, msg);
		}

		void IOStreamLogger::onLogError(c_str tag, c_str msg) {
			onLog(log_type::err, tag, msg);
		}

		void IOStreamLogger::onLogFatal(c_str tag, c_str msg) {
			onLog(log_type::fatal, tag, msg);
		}

		void IOStreamLogger::onLog(log_type type, c_str tag, c_str msg) {
			auto t = time(null);
			tm* time = localtime(&t);

			std::cout << fmt::format(strings::logs::g_logger_fmt,
				time->tm_mday,
				time->tm_mon,
				time->tm_year,

				time->tm_hour,
				time->tm_min,
				time->tm_sec,
				
				g_log_type_entries[(u8)type],
				tag,
				msg
			);
		}
	}
}