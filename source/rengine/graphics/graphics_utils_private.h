#pragma once
#include "../base_private.h"
#include <GraphicsTypes.h>
#include <EngineFactory.h>
#include <DebugOutput.h>

namespace rengine {
	namespace graphics {
		namespace utils {
			void setup_engine_create_info(Diligent::IEngineFactory* factory, u8 adapter_id, backend backend, Diligent::EngineCreateInfo& create_info);
			u32 choose_best_adapter(Diligent::IEngineFactory* factory, u8 preferred_id);
			u32 choose_adapter_with_best_memory(const vector<Diligent::GraphicsAdapterInfo>& adapters, 
				Diligent::ADAPTER_TYPE adapter_type);
			void diligent_dbg_message_helper(
				Diligent::DEBUG_MESSAGE_SEVERITY severity,
				c_str message,
				c_str function,
				c_str file,
				int line
			);
		}
	}
}