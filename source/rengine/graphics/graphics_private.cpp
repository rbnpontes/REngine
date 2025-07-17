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
#include "./texture_manager_private.h"

#include "./imgui_manager_private.h"

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
				calculate_msaa_levels,
				pipeline_state_mgr__init,
				buffer_mgr__init,
				render_target_mgr__init,
				texture_mgr__init,
				srb_mgr__init,
				allocate_buffers,
				renderer__init,
				render_command__init,
				drawing__init,
				imgui_manager__init
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
				imgui_manager__deinit,
				drawing__deinit,
				renderer__deinit,
				render_target_mgr__deinit,
				buffer_mgr__deinit,
				shader_mgr__deinit,
				srb_mgr__deinit,
				pipeline_state_mgr__deinit,
			};

			for (auto i = 0; i < _countof(deinit_calls); ++i)
				deinit_calls[i]();

			g_graphics_state.device->Release();
			g_graphics_state.factory->Release();

			core::alloc_free(g_graphics_state.contexts);
		}

		void begin() {
			profile_begin_name(strings::profiler::graphics_loop);

			const auto& window = g_engine_state.window_id;
			if (window == core::no_window)
				return;

			prepare_viewport(window);
			renderer__reset_state(false);

			verify_graphics_resources();
			prepare_swapchain_window(window);

			clear_desc desc = {};
			desc.depth = 1.0f;
			desc.clear_depth = true;
			renderer_clear(desc);

			update_buffers();
		}

		void end() {
			profile_scoped_end();
			// skip if no window has been set
			if (g_engine_state.window_id == core::no_window)
				return;

			// if no draw command has been submitted but
			// renderer has pending commands, we must
			// flush these commands before present
			if (g_renderer_state.dirty_flags != 0)
				renderer_flush();

			// if swapchain has not been created, skip then
			auto swapchain = core::window__get_swapchain(g_engine_state.window_id);
			if (!swapchain)
				return;

			blit_2_swapchain(swapchain->GetCurrentBackBufferRTV()->GetTexture());
			present_swapchain(swapchain);
		}

		void calculate_msaa_levels()
		{
			const auto backbuffer_fmt = (Diligent::TEXTURE_FORMAT)get_default_backbuffer_format();
			const auto depthbuffer_fmt = (Diligent::TEXTURE_FORMAT)get_default_depthbuffer_format();

			const auto device = g_graphics_state.device;
			// Some GPU's can contains different sample count for specific texture format
			// We must guarantee that backbuffer format and depthbuffer has the same sample count
			// even if each format has their own sample count, we must return the minimum sample
			// count that it is been supported by both.
			// Ex:
			// Backbuffer Format Sample Count = 16x
			// Depthbuffer Format Sample Count = 8x
			// MSAA Levels must be = 8x
			auto sample_counts = device->GetTextureFormatInfoExt(backbuffer_fmt).SampleCounts & device->GetTextureFormatInfoExt(depthbuffer_fmt).SampleCounts;

			u8 result = 1;
			for (u8 i = 6; i > 0; --i) {
				auto sample_count = 1 << i;
				if ((sample_counts & sample_count) == 0)
					continue;

				result = sample_count;
				break;
			}

			g_graphics_state.msaa.available_levels = result;
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

		void verify_graphics_resources()
		{
			profile();
			const auto& state = g_graphics_state;
			const auto changed_msaa = state.msaa.curr_level != state.msaa.next_level;

			if (!changed_msaa)
				return;

			// TODO: clear only MSAA pipeline states
			srb_mgr_clear_cache();
			pipeline_state_mgr_clear_cache();
		}

		void prepare_viewport(const core::window_t& window_id)
		{
			auto& state = g_graphics_state;
			const auto& wnd_size = core::window_get_size(window_id);
			auto viewport_rt = render_target_mgr_find_from_size(wnd_size);

			// TODO: apply dpi here
			state.viewport_size = wnd_size;
			prepare_viewport_rt();
		}

		void prepare_viewport_rt()
		{
			profile();
			auto& state = g_graphics_state;
			auto& viewport_size = state.viewport_size;
			const auto requires_msaa_rt = state.msaa.next_level > 1;
			const auto rt_type = requires_msaa_rt ? render_target_type::multisampling : render_target_type::normal;

			auto viewport_rt = render_target_mgr_find_from_size(viewport_size, rt_type);
			auto resolve_rt = requires_msaa_rt ? render_target_mgr_find_from_size(viewport_size, render_target_type::normal) : no_render_target;

			const auto changed_msaa = state.msaa.curr_level != state.msaa.next_level;
			const auto no_viewport_rt = viewport_rt == no_render_target;
			const auto rebuild_rt = no_viewport_rt || changed_msaa;

			if (!rebuild_rt)
				return;
			
			if (viewport_rt != no_render_target)
				render_target_mgr_destroy(viewport_rt);
			// destroy resolve copy too
			if (resolve_rt != no_render_target)
				render_target_mgr_destroy(resolve_rt);

			render_target_create_info ci{
				.desc = {
					.name = strings::graphics::g_viewport_rt_name,
					.size = viewport_size,
					.format = get_default_backbuffer_format(),
					.depth_format = get_default_depthbuffer_format(),
					.sample_count = state.msaa.next_level,
				},
				.type = rt_type
			};

			viewport_rt = render_target_mgr_create(ci);
			if (requires_msaa_rt) {
				ci.desc.name = strings::graphics::g_viewport_resolve_rt_name;
				ci.type = render_target_type::normal;
				resolve_rt = render_target_mgr_create(ci);
			}

			state.msaa.curr_level = state.msaa.next_level;
			state.viewport_rt = viewport_rt;
			state.resolve_rt = resolve_rt;
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
			{
				ResolveTextureSubresourceAttribs resolve_attribs;
				resolve_attribs.SrcTextureTransitionMode = resolve_attribs.DstTextureTransitionMode = RESOURCE_STATE_TRANSITION_MODE_TRANSITION;
				ctx->ResolveTextureSubresource(src, dst, resolve_attribs);
				return;
			}

			CopyTextureAttribs cpy_attribs = {};
			cpy_attribs.pSrcTexture = src;
			cpy_attribs.pDstTexture = dst;
			cpy_attribs.SrcTextureTransitionMode =
				cpy_attribs.DstTextureTransitionMode =
				RESOURCE_STATE_TRANSITION_MODE_TRANSITION;

			ctx->CopyTexture(cpy_attribs);
		}

		void blit_2_swapchain(Diligent::ITexture* swapchain_buffer)
		{
			profile();
			using namespace Diligent;
			const auto state = g_graphics_state;
			const auto ctx = state.contexts[0];

			const auto rt = state.viewport_rt;
			const auto resolve_rt = state.resolve_rt;
			const auto msaa_enabled = state.msaa.curr_level != 0;

			ITexture* rt_tex;
			ITexture* resolve_tex;
			rt_tex = resolve_tex = null;

			render_target_mgr__get_internal_handles(rt, &rt_tex, null);
			if (msaa_enabled)
				render_target_mgr__get_internal_handles(resolve_rt, &resolve_tex, null);

			if (msaa_enabled) {
				ResolveTextureSubresourceAttribs resolve_attr = {};
				resolve_attr.DstTextureTransitionMode =
					resolve_attr.SrcTextureTransitionMode =
					RESOURCE_STATE_TRANSITION_MODE_TRANSITION;

				ctx->ResolveTextureSubresource(rt_tex, resolve_tex, resolve_attr);

				rt_tex = resolve_tex;
			}

			const auto& desc = swapchain_buffer->GetDesc();
			Box cpy_area;
			cpy_area.MinX = cpy_area.MinY = cpy_area.MinZ = 0; 
			cpy_area.MaxX = desc.Width;
			cpy_area.MaxY = desc.Height;
			cpy_area.MaxZ = 1;

			CopyTextureAttribs cpy_attr = {};
			cpy_attr.pSrcTexture = rt_tex;
			cpy_attr.pDstTexture = swapchain_buffer;
			cpy_attr.pSrcBox = &cpy_area;
			cpy_attr.SrcTextureTransitionMode =
				cpy_attr.DstTextureTransitionMode =
				RESOURCE_STATE_TRANSITION_MODE_TRANSITION;
			ctx->CopyTexture(cpy_attr);
		}

		void update_buffers()
		{
			profile();
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

		void set_msaa_level(u8 lvl)
		{
			auto& state = g_graphics_state;
			if (math::is_power_two(lvl) && (lvl >= 1 || lvl <= state.msaa.available_levels))
				state.msaa.next_level = lvl;
			else
				io::logger_warn(strings::logs::g_graphics_tag,
					fmt::format(strings::logs::g_graphics_unsupported_msaa_level, lvl).c_str());
		}

		u8 get_msaa_level()
		{
			return g_graphics_state.msaa.curr_level;
		}

		u8 get_msaa_available_levels()
		{
			return g_graphics_state.msaa.available_levels;
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
		
		render_target_t get_viewport_rt()
		{
			return g_graphics_state.viewport_rt;
		}

		math::uvec2 get_viewport_size()
		{
			return g_graphics_state.viewport_size;
		}
	}
}