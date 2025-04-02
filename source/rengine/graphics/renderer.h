#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

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

		R_EXPORT void renderer_set_window(core::window_t window_id);
		R_EXPORT void renderer_clear(const clear_desc& desc);
		R_EXPORT void renderer_set_vbuffer(const vertex_buffer_t& buffer, u64 offset = 0);
		R_EXPORT void renderer_set_vbuffers(const vertex_buffer_t* buffers, u8 num_buffers, u64* offsets);
		R_EXPORT void renderer_set_ibuffer(const index_buffer_t& buffer);
		R_EXPORT void renderer_set_render_target(const render_target_t& rt_id, const render_target_t& depth_stencil);
		R_EXPORT void renderer_set_texture_2d(const u8& tex_slot, const texture_2d_t& tex_id);
		R_EXPORT void renderer_set_texture_3d(const u8& tex_slot, const texture_3d_t& tex_id);
		R_EXPORT void renderer_set_texture_cube(const u8& tex_slot, const texture_cube_t& tex_id);
		R_EXPORT void renderer_set_texture_array(const u8& tex_slot, const texture_array_t& tex_id);
		R_EXPORT void renderer_set_material(const material_t& material_id);
		R_EXPORT void renderer_flush();
		R_EXPORT void renderer_draw();
	}
}