#include "./graphics.h"
#include "./graphics_private.h"
#include "./diligent_private.h"
#include "./renderer_private.h"
#include "./renderer.h"
#include "./models_private.h"
#include "./buffer_manager_private.h"
#include "./render_target_manager_private.h"

#include "../rengine_private.h"
#include "../core/window_graphics_private.h"
#include "../core/window_private.h"

#include "../defines.h"
#include "../exceptions.h"
#include "../strings.h"

#include <fmt/format.h>
#include <thread>

namespace rengine {
	namespace graphics {
		graphics_state g_graphics_state = {};

		typedef void(*init_call_fn)(const graphics_init_desc&);
		typedef Diligent::ISwapChain*(*allocate_swapchain_call_fn)(const core::window_t&);

		void assert_backend(backend value) {
			c_str backend_str = strings::g_backend_strings[(u8)backend::max_backend];
			if (value < backend::max_backend)
				backend_str = strings::g_backend_strings[(u8)value];

#ifdef FEATURE_BACKEND_D3D11
			if (value == backend::d3d11)
				return;
#endif
#ifdef FEATURE_BACKEND_D3D12
			if (value == backend::d3d12)
				return;
#endif
#ifdef FEATURE_BACKEND_VULKAN
			if (value == backend::vulkan)
				return;
#endif
#ifdef FEATURE_BACKEND_WEBGPU
			if (value == backend::webgpu)
				return;
#endif
#ifdef FEATURE_BACKEND_OPENGL
			if (value == backend::opengl)
				return;
#endif

			throw graphics_exception(
				fmt::format(strings::exceptions::g_graphics_unsupported_backend, backend_str).c_str()
			);
		}

		void assert_initialization() {
			const auto initialized = g_graphics_state.factory != null
				&& g_graphics_state.contexts != null
				&& g_graphics_state.device != null
				&& g_graphics_state.num_contexts > 0;
			if (initialized)
				return;

			throw graphics_exception(strings::exceptions::g_graphics_not_initialized);
		}

		void init(const graphics_init_desc& desc)
		{
			init_call_fn init_calls[] = {
				init_d3d11,
				init_d3d12,
				init_vk,
				init_webgpu,
				init_opengl
			};

			assert_backend(desc.backend);
			g_graphics_state.backend = desc.backend;

			init_calls[(u8)g_graphics_state.backend](desc);
			assert_diligent_objects();

			buffer_mgr__init();
			render_target_mgr__init();
			models__init();

			renderer__init();
		}

		void deinit()
		{
			assert_initialization();

			renderer__deinit();
			models__deinit();
			render_target_mgr__deinit();
			buffer_mgr__deinit();

			for(u32 i = 0; i < g_graphics_state.num_contexts; ++i)
				g_graphics_state.contexts[i]->Release();
			g_graphics_state.device->Release();
			g_graphics_state.factory->Release();

			core::alloc_free(g_graphics_state.contexts);
		}

		void begin() {
			renderer__reset_state();
			
			const auto& window = g_engine_state.window_id;
			if (window == core::no_window)
				return;

			prepare_swapchain_window(window);
			prepare_viewport_rt(window);

			clear_desc desc = {};
			desc.depth = 1.0f;
			desc.clear_depth = true;
			renderer_clear(desc);
		}

		void end() {
			// if no draw command has been submitted but
			// renderer has pending commands, we must
			// flush these commands before present
			if (g_renderer_state.dirty_flags != 0)
				renderer_flush();

			// skip if no window has been set
			if (g_engine_state.window_id == core::no_window)
				return;

			// if swapchain has not been created, skip then
			auto swapchain = core::window__get_swapchain(g_engine_state.window_id);
			if (!swapchain)
				return;

			const auto& cmd = g_renderer_state.default_cmd;
			// if no render targets has been bound, do skip and finish rendering
			if (cmd.num_render_targets == 0) {
				swapchain->Present();
				return;
			}

			// if render target has been bound, we must
			// copy render target pixels to swapchain backbuffer
			// and do present. this step resolves MSAA
			auto src_rt_id = cmd.render_targets[0];
			ptr src_backbuffer = null;
			render_target_mgr_get_handlers(src_rt_id, &src_backbuffer, null);

			blit_render_targets((Diligent::ITexture*)src_backbuffer, 
				swapchain->GetCurrentBackBufferRTV()->GetTexture(), 
				false);

			swapchain->Present();
		}

		void allocate_swapchain(const core::window_t& window_id)
		{
			assert_initialization();
			if (core::window__has_swapchain(window_id))
				return;

			allocate_swapchain_call_fn allocate_calls[] = {
				allocate_window_swapchain__d3d11,
				allocate_window_swapchain__d3d12,
				allocate_window_swapchain__vk,
				allocate_window_swapchain__webgpu,
				allocate_window_swapchain__opengl
			};

			const auto swapchain = allocate_calls[(u8)g_graphics_state.backend](window_id);
			if (!swapchain)
				throw graphics_exception(strings::exceptions::g_graphics_fail_to_create_swapchain);

			core::window__put_swapchain(window_id, swapchain);
		}

		void prepare_viewport_rt(const core::window_t& window_id)
		{
			const auto& wnd_size = core::window_get_size(window_id);
			auto viewport_rt = render_target_mgr_find_from_size(wnd_size);
			if (viewport_rt == no_render_target) {
				viewport_rt = render_target_mgr_create({
					{ strings::graphics::g_viewport_rt_name, wnd_size, get_default_backbuffer_format(), get_default_depthbuffer_format() },
				});
			}

			renderer_set_render_target(viewport_rt, viewport_rt);
			renderer_set_viewport({ {0, 0}, { wnd_size.x, wnd_size.y } });
		}

		void prepare_swapchain_window(const core::window_t& window_id)
		{
			auto swapchain = core::window__get_swapchain(window_id);
			if (swapchain)
				return;

			allocate_swapchain(window_id);
		}

		void blit_render_targets(Diligent::ITexture* src, Diligent::ITexture* dst, bool msaa)
		{
			using namespace Diligent;

			const auto ctx = g_graphics_state.contexts[0];

			if (msaa)
				throw not_implemented_exception();

			CopyTextureAttribs cpy_attribs = {};
			cpy_attribs.pSrcTexture = src;
			cpy_attribs.pDstTexture = dst;
			cpy_attribs.SrcTextureTransitionMode =
				cpy_attribs.DstTextureTransitionMode =
				RESOURCE_STATE_TRANSITION_MODE_TRANSITION;

			ctx->CopyTexture(cpy_attribs);
		}

		u32 get_msaa_sample_count()
		{
			// TODO: add MSAA support
			throw not_implemented_exception();
		}

		u16 get_default_backbuffer_format()
		{
			// TODO: add support for other formats
			// Ex: Apple devices uses BGRA8_UNORM 
			return Diligent::TEX_FORMAT_RGBA8_UNORM;
		}

		u16 get_default_depthbuffer_format()
		{
			// TODO: add support for other formats
			// Ex: Apple devices uses D24_UNORM_S8_UINT if i'm not wrong
			return Diligent::TEX_FORMAT_D16_UNORM;
		}
	}
}