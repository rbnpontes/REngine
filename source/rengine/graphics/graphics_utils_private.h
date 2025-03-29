#pragma once
#include "../base_private.h"
#include <GraphicsTypes.h>

namespace rengine {
	namespace graphics {
		namespace utils {
			u32 choose_adapter_with_best_memory(const vector<Diligent::GraphicsAdapterInfo>& adapters, 
				Diligent::ADAPTER_TYPE adapter_type);
		}
	}
}