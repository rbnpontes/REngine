#include "./render_target_manager_private.h"
#include "./graphics_private.h"
#include "./graphics.h"

#include "../exceptions.h"
#include "../strings.h"

#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		render_target_mgr_state g_rt_mgr_state = {};

		void render_target_mgr__init()
		{
			g_rt_mgr_state = {};
		}

		void render_target_mgr__deinit()
		{
			render_target_mgr__clear_cache();
			g_rt_mgr_state.magic = 0;
		}

		render_target_t render_target_mgr__encode_id(u8 idx, u8 magic) {
			return idx << 8u | magic;
		}
		
		u8 render_target_mgr__decode_id(u16 value) {
			return value >> 8u;
		}

		render_target_t render_target_mgr__create(const render_target_create_info& create_desc) {
			if (create_desc.type == render_target_type::external)
				throw not_implemented_exception();

			auto& state = g_rt_mgr_state;
			if (state.count == GRAPHICS_MAX_ALLOC_RENDER_TARGETS)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_rt_mgr_reach_limit, GRAPHICS_MAX_ALLOC_RENDER_TARGETS).c_str()
				);

			const auto& idx = render_target_mgr__find_free_id();
			if (idx == MAX_U8_VALUE)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_rt_mgr_reach_limit, GRAPHICS_MAX_ALLOC_RENDER_TARGETS).c_str()
				);

			const auto device = g_graphics_state.device;
			auto& entry = state.render_targets[idx];
			entry.id = render_target_mgr__encode_id(idx, state.magic++);
			entry.desc = create_desc.desc;
			entry.type = create_desc.type;

			Diligent::ITexture* backbuffer = null;
			Diligent::ITexture* depthbuffer = null;
			render_target_mgr__alloc_textures(create_desc, &backbuffer, &depthbuffer);

			entry.backbuffer = backbuffer;
			entry.depthbuffer = depthbuffer;
			++g_rt_mgr_state.count;
			return entry.id;
		}

		void render_target_mgr__destroy(const render_target_t& id)
		{
			const auto log = g_rt_mgr_state.log;
			if (!render_target_mgr__is_valid(id)) {
				log->error(
					fmt::format(strings::logs::g_rt_mgr_cant_destroy_invalid_id, id).c_str()
				);
				return;
			}

			const auto& idx = render_target_mgr__decode_id(id);
			auto& entry = g_rt_mgr_state.render_targets[idx];

			entry.backbuffer->Release();
			if (entry.depthbuffer)
				entry.depthbuffer->Release();

			entry.backbuffer = entry.depthbuffer = null;
			entry.id = 0;
			--g_rt_mgr_state.count;
		}

		render_target_t render_target_mgr__resize(const render_target_t& id, const math::uvec2& size)
		{
			const auto& idx = render_target_mgr__assert_id(id);
			auto& entry = g_rt_mgr_state.render_targets[idx];
			if (entry.type == render_target_type::external)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_rt_mgr_cant_external, id).c_str()
				);

			auto desc = entry.backbuffer->GetDesc();
			entry.backbuffer->Release();
			if (entry.depthbuffer)
				entry.depthbuffer->Release();

			render_target_create_info ci = {
				entry.desc,
				entry.type,
				null
			};
			ci.desc.size = size;

			Diligent::ITexture* backbuffer = null;
			Diligent::ITexture* depthbuffer = null;
			render_target_mgr__alloc_textures(ci, &backbuffer, &depthbuffer);

			entry.backbuffer = backbuffer;
			entry.depthbuffer = depthbuffer;
			entry.id = render_target_mgr__encode_id(idx, ++g_rt_mgr_state.magic);
			return entry.id;
		}

		void render_target_mgr__get_desc(const render_target_t& id, render_target_desc* output_desc)
		{
			const auto& idx = render_target_mgr__assert_id(id);
			const auto& entry = g_rt_mgr_state.render_targets[idx];
			*output_desc = entry.desc;
		}

		void render_target_mgr__get_size(const render_target_t& id, math::uvec2* size)
		{
			const auto& idx = render_target_mgr__assert_id(id);
			const auto& entry = g_rt_mgr_state.render_targets[idx];
			*size = entry.desc.size;
		}

		render_target_type render_target_mgr__get_type(const render_target_t& id)
		{
			const auto& idx = render_target_mgr__assert_id(id);
			return g_rt_mgr_state.render_targets[idx].type;
		}

		bool render_target_mgr__has_depthbuffer(const render_target_t& id)
		{
			const auto& idx = render_target_mgr__assert_id(id);
			const auto& entry = g_rt_mgr_state.render_targets[id];
			return entry.depthbuffer != null;
		}

		void render_target_mgr__get_internal_handles(const render_target_t& id, Diligent::ITexture** backbuffer, Diligent::ITexture** depthbuffer)
		{
			const auto& idx = render_target_mgr__assert_id(id);
			const auto& entry = g_rt_mgr_state.render_targets[idx];

			if(backbuffer)
				*backbuffer = entry.backbuffer;
			if(depthbuffer)
				*depthbuffer = entry.depthbuffer;
		}

		render_target_t render_target_mgr__find_suitable_from_size(const math::uvec2& size)
		{
			for (const auto& entry : g_rt_mgr_state.render_targets) {
				if (!entry.backbuffer)
					continue;

				const auto& rt_size = entry.desc.size;
				if (rt_size.greater_equal_than(size))
					return entry.id;
			}

			return no_render_target;
		}

		void render_target_mgr__alloc_textures(const render_target_create_info& create_info, Diligent::ITexture** backbuffer, Diligent::ITexture** depthbuffer)
		{
			const auto device = g_graphics_state.device;

			Diligent::TextureDesc desc = {};
			desc.Name = create_info.desc.name;
			desc.Type = Diligent::RESOURCE_DIM_TEX_2D;
			desc.Width = create_info.desc.size.x;
			desc.Height = create_info.desc.size.y;
			desc.Format = (Diligent::TEXTURE_FORMAT)create_info.desc.format;
			desc.MipLevels = 1;
			desc.BindFlags = Diligent::BIND_SHADER_RESOURCE | Diligent::BIND_RENDER_TARGET;

			// MSAA texture cannot be used as shader resource
			if (create_info.type == render_target_type::multisampling) {
				desc.SampleCount = get_msaa_sample_count();
				desc.BindFlags = Diligent::BIND_RENDER_TARGET;
			}

			device->CreateTexture(desc, null, backbuffer);

			if (!*backbuffer)
				throw graphics_exception(
					strings::exceptions::g_rt_mgr_failed_to_create
				);

			(*backbuffer)->AddRef();

			if (create_info.desc.format == Diligent::TEX_FORMAT_UNKNOWN) {
				*depthbuffer = null;
				return;
			}

			desc.Format = (Diligent::TEXTURE_FORMAT)create_info.desc.depth_format;
			desc.BindFlags = Diligent::BIND_DEPTH_STENCIL | Diligent::BIND_SHADER_RESOURCE;

			// MSAA depth buffer cannot be used as shader resource
			if (create_info.type == render_target_type::multisampling)
				desc.BindFlags = Diligent::BIND_DEPTH_STENCIL;

			device->CreateTexture(desc, null, depthbuffer);

			if (!*depthbuffer)
				throw graphics_exception(
					strings::exceptions::g_rt_mgr_failed_to_create_depthbuffer
				);

			(*depthbuffer)->AddRef();
		}

		u8 render_target_mgr__find_free_id() {
			const auto& render_targets = g_rt_mgr_state.render_targets;
			for (u8 i = 0; i < GRAPHICS_MAX_RENDER_TARGETS; ++i) {
				const auto& entry = render_targets[i];
				if (entry.backbuffer == null && entry.depthbuffer == null)
					return i;
			}

			return MAX_U8_VALUE;
		}

		u8 render_target_mgr__assert_id(const render_target_t& id)
		{
			const auto& state = g_rt_mgr_state;
			const auto& idx = render_target_mgr__decode_id(id);
			if(!render_target_mgr__is_valid(id))
				throw graphics_exception(
					fmt::format(strings::exceptions::g_rt_mgr_invalid_id, id).c_str()
				);

			return idx;
		}

		bool render_target_mgr__is_valid(const render_target_t& id)
		{
			const auto& state = g_rt_mgr_state;
			const auto& idx = render_target_mgr__decode_id(id);
			if (idx >= GRAPHICS_MAX_ALLOC_RENDER_TARGETS)
				return false;
			
			const auto& entry = g_rt_mgr_state.render_targets[idx];
			return entry.id == id && entry.backbuffer;
		}

		void render_target_mgr__clear_cache()
		{
			for (auto& entry : g_rt_mgr_state.render_targets) {
				entry.id = 0;
				if (entry.backbuffer) {
					entry.backbuffer->Release();
					entry.backbuffer = null;
				}

				if (entry.depthbuffer) {
					entry.depthbuffer->Release();
					entry.depthbuffer = null;
				}
			}
		}

		u8 render_target_mgr__get_count()
		{
			return g_rt_mgr_state.count;
		}

		void render_target_mgr__get_available_rts(u8* count, render_target_t* output_ids)
		{
			if (!count)
				return;

			*count = g_rt_mgr_state.count;

			if (!output_ids)
				return;

			u8 idx = 0;
			for (const auto& entry : g_rt_mgr_state.render_targets) {
				if (!entry.backbuffer)
					continue;

				output_ids[idx] = entry.id;
				++idx;
			}

			g_rt_mgr_state.count = 0;
		}
	}
}