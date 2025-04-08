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

		R_EXPORT void renderer_clear(const clear_desc& desc);

		R_EXPORT void renderer_reset_states();
		R_EXPORT void renderer_use_command(const render_command_t& command);

		R_EXPORT void renderer_set_vbuffer(const vertex_buffer_t& buffer, u64 offset = 0);
		R_EXPORT void renderer_set_vbuffers(const vertex_buffer_t* buffers, u8 num_buffers, u64* offsets);
		R_EXPORT void renderer_set_ibuffer(const index_buffer_t& buffer);
		R_EXPORT void renderer_set_render_target(const render_target_t& rt_id);
		R_EXPORT void renderer_set_render_targets(const render_target_t* render_targets, const u8& num_rts);
		R_EXPORT void renderer_set_depthbuffer(const render_target_t& rt_id);
		R_EXPORT void renderer_set_texture_2d(const u8& tex_slot, const texture_2d_t& tex_id);
		R_EXPORT void renderer_set_texture_3d(const u8& tex_slot, const texture_3d_t& tex_id);
		R_EXPORT void renderer_set_texture_cube(const u8& tex_slot, const texture_cube_t& tex_id);
		R_EXPORT void renderer_set_texture_array(const u8& tex_slot, const texture_array_t& tex_id);
		R_EXPORT void renderer_set_pipeline(const pipeline_state_t& pipeline_id);

		R_EXPORT void renderer_set_viewport(const math::urect& rect);
		R_EXPORT void renderer_set_topology(const primitive_topology& topology);
		R_EXPORT void renderer_set_cull_mode(const cull_mode& cull);
		R_EXPORT void renderer_set_vertex_elements(const u32& vertex_elements);
		R_EXPORT void renderer_set_vertex_shader(const shader_t& shader_id);
		R_EXPORT void renderer_set_pixel_shader(const shader_t& shader_id);
		R_EXPORT void renderer_set_depth_enabled(const bool& enabled);

		R_EXPORT void renderer_set_dbg_name(c_str dbg_name);

		R_EXPORT void renderer_set_material(const material_t& material_id);
		R_EXPORT void renderer_flush();
		R_EXPORT void renderer_draw();

		R_EXPORT void renderer_blit(const render_target_t& src, const render_target_t& dst);
	}
}