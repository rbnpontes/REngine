#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <rengine/graphics/pipeline_state_manager.h>
#include <rengine/math/math-types.h>

namespace rengine {
	namespace graphics {
		struct clear_desc {
			u8 render_target_index{ 0 };
			u8 stencil{ 1 };
			bool clear_depth{ false };
			bool clear_stencil{ false };
			float color[4] RENDERER_DEFAULT_CLEAR_COLOR;
			float depth{ 0.0f };
		};

		struct draw_desc {
			u32 num_vertices{ 0 };
			u32 num_instances{ 1 };
			u32 start_vertex_idx{ 0 };
			u32 start_instance_idx{ 0 };
		};

		struct draw_indexed_desc {
			u32 num_indices{ 0 };
			u32 num_instances{ 1 };
			u32 start_vertex_idx{ 0 };
			u32 start_index_idx{ 0 };
			u32 start_instance_idx{ 0 };
			bool use_32bit_indices{ false };
		};

		R_EXPORT void renderer_clear(const clear_desc& desc);

		R_EXPORT void renderer_reset_states();
		R_EXPORT void renderer_use_command(const render_command_t& command);

		R_EXPORT void renderer_set_vbuffer(const vertex_buffer_t& buffer, u64 offset = 0);
		R_EXPORT void renderer_set_vbuffers(const vertex_buffer_t* buffers, u8 num_buffers, u64* offsets);
		R_EXPORT void renderer_set_ibuffer(const index_buffer_t& buffer, u64 offset = 0);
		R_EXPORT void renderer_set_render_target(const render_target_t& rt_id, const render_target_t& depth_id = no_render_target);
		R_EXPORT void renderer_set_render_targets(const render_target_t* render_targets, const u8& num_rts, const render_target_t& depth_id = no_render_target);
        R_EXPORT void renderer_set_texture_2d(c_str slot_name, const texture_2d_t& tex_id);
        R_EXPORT void renderer_set_texture_3d(c_str slot_name, const texture_3d_t& tex_id);
        R_EXPORT void renderer_set_texture_cube(c_str slot_name, const texture_cube_t& tex_id);
        R_EXPORT void renderer_set_texture_array(c_str slot_name, const texture_array_t& tex_id);
        R_EXPORT void renderer_set_texture_2d(core::hash_t slot, const texture_2d_t& tex_id);
        R_EXPORT void renderer_set_texture_3d(core::hash_t slot, const texture_3d_t& tex_id);
        R_EXPORT void renderer_set_texture_cube(core::hash_t slot, const texture_cube_t& tex_id);
        R_EXPORT void renderer_set_texture_array(core::hash_t slot, const texture_array_t& tex_id);

		R_EXPORT void renderer_set_viewport(const math::urect& rect);
		R_EXPORT void renderer_set_topology(const primitive_topology& topology);
                R_EXPORT void renderer_set_cull_mode(const cull_mode& cull);
                R_EXPORT void renderer_set_program(const shader_program_t& program_id);
                R_EXPORT void renderer_set_depth_enabled(const bool& enabled);
                R_EXPORT void renderer_set_depth_write(const bool& enabled);
                R_EXPORT void renderer_set_stencil_test(const bool& enabled);
                R_EXPORT void renderer_set_depth_cmp_func(const comparison_function& func);
                R_EXPORT void renderer_set_stencil_cmp_func(const comparison_function& func);
                R_EXPORT void renderer_set_stencil_pass_op(const stencil_op& op);
                R_EXPORT void renderer_set_stencil_fail_op(const stencil_op& op);
                R_EXPORT void renderer_set_depth_fail_op(const stencil_op& op);
                R_EXPORT void renderer_set_stencil_cmp_mask(u8 mask);
                R_EXPORT void renderer_set_stencil_write_mask(u8 mask);
                R_EXPORT void renderer_set_blend_mode(const blend_mode& mode);
                R_EXPORT void renderer_set_color_write(const bool enabled);
                R_EXPORT void renderer_set_alpha_to_coverage(const bool enabled);
                R_EXPORT void renderer_set_scissors(const bool enabled);
                R_EXPORT void renderer_set_constant_depth_bias(float bias);
                R_EXPORT void renderer_set_slope_scaled_depth_bias(float bias);
                R_EXPORT void renderer_set_wireframe(const bool enabled);

		R_EXPORT void renderer_set_material(const material_t& material_id);
		R_EXPORT void renderer_flush();
		R_EXPORT void renderer_draw(const draw_desc& desc);
		R_EXPORT void renderer_draw_indexed(const draw_indexed_desc& desc);

		R_EXPORT void renderer_blit(const render_target_t& src, const render_target_t& dst);
	}
}