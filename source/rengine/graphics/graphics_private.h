#pragma once
#include "../base_private.h"
#include "../math/math-types.h"
#include "../math/matrix4x4.h"

#include <EngineFactory.h>
#include <DeviceContext.h>
#include <RenderDevice.h>
#include <GraphicsTypes.h>
#include <MemoryAllocator.h>

#define GRAPHICS_VERSION Diligent::Version { 11, 0 }

namespace rengine {
	namespace graphics {
		struct diligent_allocator : Diligent::IMemoryAllocator {
			ptr Allocate(size_t size, c_str dbg_desc, c_str dbg_file_name, const i32 dbg_line_num) override;
			void Free(ptr mem) override;
		};
		
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

		struct graphics_msaa {
			u8 available_levels{ 1 };
			u8 curr_level{ 1 };
			u8 next_level{ 1 };
		};

		struct graphics_state {
			Diligent::IEngineFactory* factory;
			Diligent::IDeviceContext** contexts;
			Diligent::IRenderDevice* device;
			diligent_allocator* allocator;
			u32 num_contexts;
			render_target_t viewport_rt;
			render_target_t resolve_rt;
			math::uvec2 viewport_size;
			backend backend;
			graphics_buffers buffers;

			bool vsync{ false };
			graphics_msaa msaa{};
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

		void calculate_msaa_levels();
		void allocate_swapchain(const core::window_t& window_id);
		void allocate_buffers();
		void verify_graphics_resources();
		void prepare_viewport(const core::window_t& window_id);
		void prepare_viewport_rt();
		void prepare_swapchain_window(const core::window_t& window_id);
		
		void blit_2_swapchain(Diligent::ITexture* swapchain_buffer);

		void update_buffers();
		void update_frame_buffer();

		void present_swapchain(Diligent::ISwapChain* swapchain);
	}
}