#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

namespace rengine {
	namespace graphics {
		struct sampler_desc {
			filter_type filter{ filter_type::linear };
			texture_address_mode address{ texture_address_mode::clamp };
			float lod_bias{ 0.0f };
			float min_lod{ 0.0f };
			float max_lod{ MAX_FLOAT_VALUE };
			u32 max_anisotropy{ 0 };
			comparison_function comparison{ comparison_function::never };
		};
		struct immutable_sampler_desc {
			c_str name{ null };
			u32 shader_type_flags{ (u32)shader_type_flags::none };
			sampler_desc desc;
		};

		struct depth_desc {
			bool depth_enabled{ true };
			bool depth_write{ true };
			bool stencil_test{ false };
			
			comparison_function depth_cmp_func{ comparison_function::less_equal };
			comparison_function stencil_cmp_func{ comparison_function::always };
			
			stencil_op on_passed{ stencil_op::keep };
			stencil_op on_stencil{ stencil_op::keep };
			stencil_op on_depth_fail{ stencil_op::keep };

			u8 cmp_mask{ 255 };
			u8 write_mask{ 255 };
		};

		struct graphics_pipeline_state_create {
			c_str name{ null };
			u16 render_target_formats[GRAPHICS_MAX_RENDER_TARGETS];
			u16 depth_stencil_format{ 0 };
			u8 num_render_targets{ 0 };
			primitive_topology topology{ primitive_topology::triangle_list };
			cull_mode cull{ cull_mode::clock_wise };
			u8 msaa_level{ 1 };
			blend_mode blend_mode{ blend_mode::replace };
			depth_desc depth_desc{};
			bool color_write{ true };
			bool alpha_to_coverage{ false };
			bool wireframe{ false };
			bool scissors{ false };
			float constant_depth_bias{ 0.0f };
			float slope_scaled_depth_bias{ 0.0f };
			shader_program_t shader_program{ no_shader_program };
			immutable_sampler_desc* immutable_samplers{ null };
			u32 num_immutable_samplers{ 0 };
		};

		R_EXPORT pipeline_state_t pipeline_state_mgr_create_graphics(const graphics_pipeline_state_create& create_info);
		R_EXPORT core::hash_t pipeline_state_mgr_graphics_hash_desc(const graphics_pipeline_state_create& create_info);
		R_EXPORT ptr pipeline_state_mgr_get_internal_handle(pipeline_state_t id);
		R_EXPORT u32 pipeline_state_mgr_get_cache_count();
		R_EXPORT void pipeline_state_mgr_clear_cache();
	}
}