#pragma once
#include "../base_private.h"
#include "./render_target_manager.h"

#include "../core/pool.h"
#include "../io/logger.h"

#include <SwapChain.h>
#include <Texture.h>

namespace rengine {
	namespace graphics {
		struct render_target_entry {
			render_target_type type{ render_target_type::normal };
			render_target_desc desc{};
			Diligent::ITexture* backbuffer{ null };
			Diligent::ITexture* depthbuffer{ null };
		};

		struct render_target_mgr_state {
			io::ILog* log;
			core::array_pool<render_target_entry, GRAPHICS_MAX_ALLOC_RENDER_TARGETS> render_targets;
			/*u8 count{ 0 };
			u8 magic{ 0 };*/
		};

		extern render_target_mgr_state g_rt_mgr_state;

		void render_target_mgr__init();
		void render_target_mgr__deinit();

		render_target_t render_target_mgr__encode_id(u8 idx, u8 magic);
		u8 render_target_mgr__decode_id(u16 value);

		render_target_t render_target_mgr__create(const render_target_create_info& create_desc);
		void render_target_mgr__destroy(const render_target_t& id);
		render_target_t render_target_mgr__resize(const render_target_t& id, const math::uvec2& size);
		void render_target_mgr__get_desc(const render_target_t& id, render_target_desc* output_desc);
		void render_target_mgr__get_size(const render_target_t& id, math::uvec2* size);
		render_target_type render_target_mgr__get_type(const render_target_t& id);
		bool render_target_mgr__has_depthbuffer(const render_target_t& id);
		void render_target_mgr__get_internal_handles(const render_target_t& id, Diligent::ITexture** backbuffer, Diligent::ITexture** depthbuffer);
		render_target_t render_target_mgr__find_suitable_from_size(const math::uvec2& size);


		void render_target_mgr__alloc_textures(const render_target_create_info& create_info, Diligent::ITexture** backbuffer, Diligent::ITexture** depthbuffer);
		bool render_target_mgr__is_valid(const render_target_t& id);

		void render_target_mgr__clear_cache();
		u8 render_target_mgr__get_count();
		void render_target_mgr__get_available_rts(u8* count, render_target_t* output_ids);
	}
}