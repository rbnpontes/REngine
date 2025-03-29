#include "./graphics_utils_private.h"

namespace rengine {
	namespace graphics {
		namespace utils {
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
		}
	}
}