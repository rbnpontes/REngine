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
			//g_rt_mgr_state.magic = 0;
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

			const auto device = g_graphics_state.device;
			render_target_entry entry;
			entry.desc = create_desc.desc;
			entry.type = create_desc.type;

			render_target_mgr__alloc_textures(create_desc, &entry.backbuffer, &entry.depthbuffer);
			return g_rt_mgr_state.render_targets.push_back(entry);
		}

		void render_target_mgr__destroy(const render_target_t& id)
		{
			auto& entry = g_rt_mgr_state.render_targets[id];

			entry.value.backbuffer->Release();
			if (entry.value.depthbuffer)
				entry.value.depthbuffer->Release();
			g_rt_mgr_state.render_targets.erase(id);
		}

		render_target_t render_target_mgr__resize(const render_target_t& id, const math::uvec2& size)
		{
			//const auto& idx = render_target_mgr__assert_id(id);
			auto& entry = g_rt_mgr_state.render_targets[id];
			if (entry.value.type == render_target_type::external)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_rt_mgr_cant_external, id).c_str()
				);

			auto desc = entry.value.backbuffer->GetDesc();
			entry.value.backbuffer->Release();
			if (entry.value.depthbuffer)
				entry.value.depthbuffer->Release();

			render_target_create_info ci = {
				entry.value.desc,
				entry.value.type,
				null
			};
			ci.desc.size = size;

			Diligent::ITexture* backbuffer = null;
			Diligent::ITexture* depthbuffer = null;
			render_target_mgr__alloc_textures(ci, &backbuffer, &depthbuffer);

			render_target_entry resized_rt_entry = { 
				entry.value.type,
				entry.value.desc, 
				backbuffer,
				depthbuffer
			};
			return g_rt_mgr_state.render_targets.replace(id, resized_rt_entry);
		}

		void render_target_mgr__get_desc(const render_target_t& id, render_target_desc* output_desc)
		{
			const auto& entry = g_rt_mgr_state.render_targets[id];
			*output_desc = entry.value.desc;
		}

		void render_target_mgr__get_size(const render_target_t& id, math::uvec2* size)
		{
			const auto& entry = g_rt_mgr_state.render_targets[id];
			*size = entry.value.desc.size;
		}

		render_target_type render_target_mgr__get_type(const render_target_t& id)
		{
			return g_rt_mgr_state.render_targets[id].value.type;
		}

		bool render_target_mgr__has_depthbuffer(const render_target_t& id)
		{
			const auto& entry = g_rt_mgr_state.render_targets[id];
			return entry.value.depthbuffer != null;
		}

		void render_target_mgr__get_internal_handles(const render_target_t& id, Diligent::ITexture** backbuffer, Diligent::ITexture** depthbuffer)
		{
			const auto& entry = g_rt_mgr_state.render_targets[id];

			if(backbuffer)
				*backbuffer = entry.value.backbuffer;
			if(depthbuffer)
				*depthbuffer = entry.value.depthbuffer;
		}

		render_target_t render_target_mgr__find_suitable_from_size(const math::uvec2& size, render_target_type expected_type = render_target_type::normal)
		{
			for (const auto& entry : g_rt_mgr_state.render_targets) {
				if (!entry.value.backbuffer || entry.value.type != expected_type)
					continue;

				const auto& rt_size = entry.value.desc.size;
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
				desc.SampleCount = create_info.desc.sample_count;
				desc.BindFlags = Diligent::BIND_RENDER_TARGET;
			}

			device->CreateTexture(desc, null, backbuffer);

			if (!*backbuffer)
				throw graphics_exception(
					strings::exceptions::g_rt_mgr_failed_to_create
				);

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
		}

		bool render_target_mgr__is_valid(const render_target_t& id)
		{
			const auto& state = g_rt_mgr_state;
			return state.render_targets.is_valid(id);
		}

		void render_target_mgr__clear_cache()
		{
			for (auto& entry : g_rt_mgr_state.render_targets) {
				if (entry.value.backbuffer)
					entry.value.backbuffer->Release();

				if (entry.value.depthbuffer)
					entry.value.depthbuffer->Release();
			}

			g_rt_mgr_state.render_targets.clear();
		}

		u8 render_target_mgr__get_count()
		{
			return g_rt_mgr_state.render_targets.count();
		}

		void render_target_mgr__get_available_rts(u8* count, render_target_t* output_ids)
		{
			if (!count)
				return;

			*count = g_rt_mgr_state.render_targets.count();

			if (!output_ids)
				return;

			u8 idx = 0;
			for (const auto& entry : g_rt_mgr_state.render_targets) {
				output_ids[idx] = entry.id;
				++idx;
			}
		}
	}
}