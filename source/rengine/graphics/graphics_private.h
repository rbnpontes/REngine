#pragma once
#include "../base_private.h"

#include <EngineFactory.h>
#include <DeviceContext.h>
#include <RenderDevice.h>
#include <GraphicsTypes.h>

PRIVATE_HEADER
namespace rengine {
	namespace graphics {
		struct graphics_state {
			Diligent::IEngineFactory* factory;
			Diligent::IDeviceContext* context;
			Diligent::IRenderDevice* device;
			backend backend;
		};

		struct graphics_init_desc {
			core::window_t window_id;
			backend backend;
		};

		graphics_state* get_state();
		void assert_backend(backend value);

		void init(const graphics_init_desc& desc);

		void init_d3d11(const graphics_init_desc& desc);
		void init_d3d12(const graphics_init_desc& desc);
		void init_vk(const graphics_init_desc& desc);
		void init_webgpu(const graphics_init_desc& desc);
		void init_opengl(const graphics_init_desc& desc);

		void setup_engine_create_info(const graphics_init_desc& desc, Diligent::EngineCreateInfo& create_info);
		u32 choose_best_adapter();
	}
}