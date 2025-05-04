#pragma once
#include "../base_private.h"
#include "../math/math-types.h"
#include "../math/matrix4x4.h"

#include <EngineFactory.h>
#include <DeviceContext.h>
#include <RenderDevice.h>
#include <GraphicsTypes.h>

#define GRAPHICS_VERSION Diligent::Version { 11, 0 }

namespace rengine {
	namespace graphics {
		struct frame_buffer_data {
			math::matrix4x4 screen_projection;
			math::vec2 window_size;
			number_t delta_time;
			number_t elapsed_time;
			u32 frame;
		};
		
		struct graphics_buffers {
			constant_buffer_t frame{ no_constant_buffer };
		};

		struct graphics_state {
			Diligent::IEngineFactory* factory;
			Diligent::IDeviceContext** contexts;
			Diligent::IRenderDevice* device;
			u32 num_contexts;
			render_target_t viewport_rt;
			backend backend;

			graphics_buffers buffers;
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
		void allocate_buffers();
		void prepare_viewport_rt(const core::window_t& window_id);
		void prepare_swapchain_window(const core::window_t& window_id);
		void blit_render_targets(Diligent::ITexture* src, Diligent::ITexture* dst, bool msaa);

		void update_buffers();
		void update_frame_buffer();
	}
}