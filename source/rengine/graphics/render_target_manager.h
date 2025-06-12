#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <rengine/math/math-types.h>

namespace rengine {
	namespace graphics {
		enum class render_target_type : u8 {
			normal = 0,
			multisampling,
			external
		};

		struct render_target_desc {
			c_str name{ null };
			math::uvec2 size;
			u16 format{ 0 };
			u16 depth_format{ 0 };
			u8 sample_count{ 1 };
		};

		struct render_target_external_desc {
			ptr backbuffer;
			ptr depthbuffer;
		};

		struct render_target_create_info {
			render_target_desc desc{};
			render_target_type type{ render_target_type::normal };
			render_target_external_desc* external_desc{ null };
		};

		R_EXPORT render_target_t render_target_mgr_create(const render_target_create_info& create_desc);
		R_EXPORT void render_target_mgr_destroy(const render_target_t& id);
		R_EXPORT render_target_t render_target_mgr_resize(const render_target_t& id, const math::uvec2& size);
		R_EXPORT void render_target_mgr_get_size(const render_target_t& id, math::uvec2* size);
		R_EXPORT void render_target_mgr_get_desc(const render_target_t& id, render_target_desc* output_desc);
		R_EXPORT render_target_type render_target_mgr_get_type(const render_target_t& id);
		R_EXPORT bool render_target_mgr_has_depthbuffer(const render_target_t& id);
		R_EXPORT void render_target_mgr_get_handlers(const render_target_t& id, ptr* backbuffer, ptr* depthbuffer);
		R_EXPORT u8 render_target_mgr_get_count();
		R_EXPORT void render_target_mgr_clear_cache();
		R_EXPORT void render_target_mgr_get_available_rts(u8* count, render_target_t* output_ids);
		R_EXPORT bool render_target_mgr_is_valid(const render_target_t& id);
		R_EXPORT render_target_t render_target_mgr_find_from_size(const math::uvec2& size, render_target_type expected_type = render_target_type::normal);
	}
}