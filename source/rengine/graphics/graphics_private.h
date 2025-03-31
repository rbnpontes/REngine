#pragma once
#include "../base_private.h"

#include <EngineFactory.h>
#include <DeviceContext.h>
#include <RenderDevice.h>
#include <GraphicsTypes.h>

#define GRAPHICS_VERSION Diligent::Version { 11, 0 }

namespace rengine {
	namespace graphics {
		struct graphics_state {
			Diligent::IEngineFactory* factory;
			Diligent::IDeviceContext** contexts;
			Diligent::IRenderDevice* device;
			u32 num_contexts;
			backend backend;
		};

		struct graphics_init_desc {
			core::window_t window_id;
			u8 adapter_id;
			backend backend;
		};

		extern graphics_state g_graphics_state;

		void assert_backend(backend value);
		void assert_initialization();

		void init(const graphics_init_desc& desc);
		void deinit();

		void begin();
		void end();

		void allocate_swapchain(const core::window_t& window_id);
	}
}