#include "./graphics_utils_private.h"
#include "./graphics_private.h"

#include "../exceptions.h"
#include "../io/logger.h"

#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		namespace utils {
			void setup_engine_create_info(Diligent::IEngineFactory* factory, u8 adapter_id, backend backend, Diligent::EngineCreateInfo& create_info)
			{
				create_info.GraphicsAPIVersion = GRAPHICS_VERSION;
#if _DEBUG
				create_info.EnableValidation = true;
				create_info.SetValidationLevel(Diligent::VALIDATION_LEVEL_2);
#endif
				create_info.AdapterId = choose_best_adapter(factory, adapter_id);
				create_info.NumDeferredContexts = std::thread::hardware_concurrency();
				if (backend == backend::opengl)
					create_info.NumDeferredContexts = 0;
			}

			u32 choose_best_adapter(Diligent::IEngineFactory* factory, u8 preferred_id)
			{
				u32 num_adapters;
				factory->EnumerateAdapters(GRAPHICS_VERSION, num_adapters, null);

				if (num_adapters == 0)
					throw graphics_exception(strings::exceptions::g_graphics_not_suitable_device);

				vector<Diligent::GraphicsAdapterInfo> adapters(num_adapters);
				factory->EnumerateAdapters(GRAPHICS_VERSION, num_adapters, adapters.data());

				if (preferred_id < adapters.size()) {
					const auto& adapter = adapters[preferred_id];
					if (adapter.Type == adapter.Type == Diligent::ADAPTER_TYPE_UNKNOWN)
						throw graphics_exception(strings::exceptions::g_graphics_unknow_adapter);
					return preferred_id;
				}
				else if(preferred_id != MAX_U8_VALUE) {
					io::logger_warn(strings::logs::g_graphics_tag, fmt::format(strings::logs::g_graphics_invalid_adapter_id, preferred_id).c_str());
				}

				u32 result = utils::choose_adapter_with_best_memory(adapters, Diligent::ADAPTER_TYPE_DISCRETE);
				if (result != MAX_U32_VALUE)
					return result;

				result = utils::choose_adapter_with_best_memory(adapters, Diligent::ADAPTER_TYPE_INTEGRATED);
				if (result != MAX_U32_VALUE)
					return result;

				result = 0;
				io::logger_warn(strings::logs::g_graphics_tag, strings::logs::g_graphics_no_suitable_device_found);
				return result;
			}

			u32 choose_adapter_with_best_memory(const vector<Diligent::GraphicsAdapterInfo>& adapters,
				Diligent::ADAPTER_TYPE adapter_type) {
				u32 last_total_memory, result;
				last_total_memory = result = 0;

				for (u32 i = 0; i < adapters.size(); ++i) {
					const auto& adapter = adapters[i];
					if (adapter.Type != adapter_type)
						continue;

					const u32 total_memory = adapter.Memory.HostVisibleMemory + adapter.Memory.LocalMemory;
					if (total_memory < last_total_memory)
						continue;

					result = i;
					last_total_memory = total_memory;
				}

				return result;
			}

			void diligent_dbg_message_helper(Diligent::DEBUG_MESSAGE_SEVERITY severity, c_str message, c_str function, c_str file, int line)
			{
				io::logger_call_fn log_calls[] = {
					io::logger_info,
					io::logger_warn,
					io::logger_error,
					io::logger_fatal
				};

				if (severity > Diligent::DEBUG_MESSAGE_SEVERITY_FATAL_ERROR)
					return;

				
				const auto log_call_idx = (i8)severity;
				const auto& log_call = log_calls[log_call_idx];
				const auto msg = fmt::format(strings::logs::g_graphics_diligent_dbg_fmt,
					message,
					function,
					file,
					line);
				log_call(
					strings::logs::g_diligent_tag,
					msg.c_str()
				);
			}
		}
	}
}