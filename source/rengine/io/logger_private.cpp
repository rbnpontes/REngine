#include "./logger_private.h"
#include "./logger_iostream_impl_private.h"

#include "../core/allocator.h"
#include "../strings.h"
#include "../exceptions.h"

#include <fmt/format.h>

namespace rengine {
	namespace io {
		logger_state g_logger_state = {};

		class InternalLog : public ILog {
		public:
			InternalLog(const string& tag) : tag_(tag) {}
			void info(c_str msg) override {
				logger_info(tag_.c_str(), msg);
			}
			void warn(c_str msg) override {
				logger_warn(tag_.c_str(), msg);
			}
			void error(c_str msg) override {
				logger_error(tag_.c_str(), msg);
			}
			void fatal(c_str msg) override {
				logger_fatal(tag_.c_str(), msg);
			}
			ILog* use(c_str sub_tag) override {
				if (sub_tag == null)
					return this;
				return logger__alloc_log(string(fmt::format("{0}::{1}", tag_, sub_tag).c_str()));
			}
			string tag_;
		};

		void logger__init() {
			g_logger_state.current_logger = core::alloc_new<IOStreamLogger>();
		}

		void logger__deinit()
		{
			auto& state = g_logger_state;

			if(state.current_logger != null)
				core::alloc_free(state.current_logger);

			// de-alloc all logs 
			while (!state.logs.empty()) {
				core::alloc_free(state.logs.front());
				state.logs.pop();
			}

			g_logger_state = {};
		}

		void logger__assert_logger() {
			if (g_logger_state.current_logger)
				return;
			throw null_exception(
				fmt::format(strings::exceptions::g_null_object, "Logger").c_str()
			);
		}

		void logger__change_default_logger(ILogger* logger)
		{
			if (g_logger_state.current_logger)
				core::alloc_free(g_logger_state.current_logger);
			g_logger_state.current_logger = logger;
		}

		ILog* logger__alloc_log(const string& tag)
		{
			if(g_logger_state.num_logs == IO_MAX_LOG_OBJECTS)
				throw io::logger_exception(
					fmt::format(strings::exceptions::g_logger_reached_max_log_objects).c_str()
				);

			ILog* log_obj = core::alloc_new<InternalLog>(tag);
			g_logger_state.logs.push(log_obj);
			++g_logger_state.num_logs;
			return log_obj;
		}
	}
}