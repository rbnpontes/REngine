#include "./graphics.h"
#include "./graphics_private.h"
#include "./diligent_private.h"
#include "./renderer_private.h"
#include "./renderer.h"
#include "./models_private.h"
#include "./drawing_private.h"
#include "./buffer_manager_private.h"
#include "./render_target_manager_private.h"
#include "./pipeline_state_manager_private.h"
#include "./srb_manager_private.h"
#include "./render_command_private.h"
#include "./shader_manager_private.h"

#include "../rengine_private.h"
#include "../core/window_graphics_private.h"
#include "../core/window_private.h"
#include "../core/profiler.h"

#include "../defines.h"
#include "../exceptions.h"
#include "../strings.h"

#include <fmt/format.h>
#include <thread>

namespace rengine {
	namespace graphics {
		graphics_state g_graphics_state = {};

		typedef void(*init_call_fn)(const graphics_init_desc&);
		typedef Diligent::ISwapChain* (*allocate_swapchain_call_fn)(const core::window_t&);

#if ENGINE_DEBUG
		struct diligent_memory_header {
			c_str desc;
			c_str file_name;
			i32 line;
		};
#endif
		ptr diligent_allocator::Allocate(size_t size, c_str dbg_desc, c_str dbg_file_name, const i32 dbg_line_num) {
#if ENGINE_DEBUG
			const auto curr_size = size;
			size_t dbg_desc_len = 0;
			size_t dbg_file_name_len = 0;
			size += sizeof(diligent_memory_header);
			if (dbg_desc != null) {
				dbg_desc_len = strlen(dbg_desc) + 1;
				size += dbg_desc_len * sizeof(char);
			}
			if (dbg_file_name != null) {
				dbg_file_name_len = strlen(dbg_file_name) + 1;
				size += dbg_file_name_len * sizeof(char);
			}
#endif
			ptr raw_mem = core::alloc(size);
#if ENGINE_DEBUG
			diligent_memory_header* header = static_cast<diligent_memory_header*>(raw_mem);
			u8* target_mem = static_cast<u8*>(raw_mem) + sizeof(diligent_memory_header);
			// memory must write after header
			raw_mem = target_mem;

			// offset to dbg desc
			target_mem += curr_size;
			if (dbg_desc != null) {
				strcpy((char*)target_mem, (c_str)dbg_desc);
				header->desc = (c_str)target_mem;
			}
			else
				header->desc = null;

			// offset to dbg_desc
			target_mem += dbg_desc_len;
			if (dbg_file_name != null) {
				strcpy((char*)target_mem, (c_str)dbg_file_name);
				header->file_name = (c_str)target_mem;
			}
			else
				header->file_name = null;

			header->line = dbg_line_num;
#endif
			return raw_mem;
		}

		void diligent_allocator::Free(ptr mem) {
			if (!mem)
				return;
#if ENGINE_DEBUG
			auto header = (diligent_memory_header*)((u8*)mem - sizeof(diligent_memory_header));
			mem = header;
#endif
			core::alloc_free(mem);
		}

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
			g_graphics_state.allocator = core::alloc_new<diligent_allocator>();

			init_call_fn init_graphics_calls[] = {
				init_d3d11,
				init_d3d12,
				init_vk,
				init_webgpu,
				init_opengl
			};
			action_t init_calls[] = {
				buffer_mgr__init,
				render_target_mgr__init,
				srb_mgr__init,
				allocate_buffers,
				renderer__init,
				render_command__init,
				drawing__init,
			};

			assert_backend(desc.backend);
			g_graphics_state.backend = desc.backend;

			init_graphics_calls[(u8)g_graphics_state.backend](desc);
			assert_diligent_objects();

			for (auto i = 0; i < _countof(init_calls); ++i)
				init_calls[i]();
		}

		void deinit()
		{
			assert_initialization();

			action_t deinit_calls[] = {
				renderer__deinit,
				render_target_mgr__deinit,
				buffer_mgr__deinit,
				shader_mgr__deinit,
				srb_mgr__deinit,
				pipeline_state_mgr__deinit,
			};

			for (u32 i = 0; i < g_graphics_state.num_contexts; ++i)
				g_graphics_state.contexts[i]->Release();
			g_graphics_state.device->Release();
			g_graphics_state.factory->Release();

			core::alloc_free(g_graphics_state.contexts);
		}

		void begin() {
			profile_begin_name(strings::profiler::graphics_loop);

			renderer__reset_state(false);

			const auto& window = g_engine_state.window_id;
			if (window == core::no_window)
				return;

			prepare_swapchain_window(window);
			prepare_viewport_rt(window);

			clear_desc desc = {};
			desc.depth = 1.0f;
			desc.clear_depth = true;
			renderer_clear(desc);

			update_buffers();
		}

		void end() {
			profile_scoped_end();
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
				present_swapchain(swapchain);
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

			present_swapchain(swapchain);
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

		void allocate_buffers()
		{
			g_graphics_state.buffers.frame = buffer_mgr_cbuffer_create({
				strings::graphics::g_frame_buffer_name,
				sizeof(frame_buffer_data),
				null,
				true
				});
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
			profile();
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

		void update_buffers()
		{
			update_frame_buffer();
		}

		void update_frame_buffer()
		{
			if (g_graphics_state.buffers.frame == no_constant_buffer)
				return;

			const auto& state = g_engine_state;
			const auto& wnd_size = core::window_get_size(g_engine_state.window_id);
			const auto& duration = std::chrono::duration_cast<std::chrono::milliseconds>(state.time.curr_elapsed.time_since_epoch());

			const auto wnd_size_vec = math::vec2(wnd_size.x, wnd_size.y);
			frame_buffer_data data;
			data.screen_projection = math::matrix4x4::screen_projection(wnd_size_vec);
			data.frame = (u32)state.time.curr_frame;
			data.delta_time = state.time.curr_delta;
			data.elapsed_time = duration.count();
			data.window_size = wnd_size_vec;

			buffer_mgr_cbuffer_update(g_graphics_state.buffers.frame, &data, sizeof(frame_buffer_data));
		}

		void present_swapchain(Diligent::ISwapChain* swapchain)
		{
			profile();
			swapchain->Present(g_graphics_state.vsync ? 1 : 0);
		}

		void enable_vsync()
		{
			g_graphics_state.vsync = true;
		}

		void disable_vsync()
		{
			g_graphics_state.vsync = false;
		}

		bool vsync_enabled()
		{
			return g_graphics_state.vsync;
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