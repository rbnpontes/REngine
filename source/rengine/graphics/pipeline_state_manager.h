#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

namespace rengine {
	namespace graphics {
		struct graphics_pipeline_state_create {
			c_str name{ null };
			u16 render_target_formats[GRAPHICS_MAX_RENDER_TARGETS];
			u16 depth_stencil_format{ 0 };
			u8 num_render_targets{ 0 };
			primitive_topology topology{ primitive_topology::triangle_list };
			cull_mode cull{ cull_mode::clock_wise };
			u8 msaa_level{ 1 };
			u32 vertex_elements{ (u32)vertex_elements::none };
			bool depth{ true };
			bool wireframe{ false };
			bool scissors{ false };
			// TODO: add more shader types
			shader_t vertex_shader{ no_shader };
			shader_t pixel_shader{ no_shader };
		};

		R_EXPORT pipeline_state_t pipeline_state_mgr_create_graphics(const graphics_pipeline_state_create& create_info);
		R_EXPORT core::hash_t pipeline_state_mgr_graphics_hash_desc(const graphics_pipeline_state_create& create_info);
		R_EXPORT ptr pipeline_state_mgr_get_internal_handle(pipeline_state_t id);
		R_EXPORT u32 pipeline_state_mgr_get_cache_count();
		R_EXPORT void pipeline_state_mgr_clear_cache();
	}
}