#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <rengine/math/math-types.h>

namespace rengine {
	namespace graphics {
		void render_command_begin(c_str cmd_name = null);
		void render_command_destroy(const render_command_t& command);
		render_command_t render_command_end();
		void render_command_begin_update(const render_command_t& command);
		void render_command_set_vbuffer(const vertex_buffer_t& buffer, const u64& offset = 0);
		void render_command_set_vbuffers(const vertex_buffer_t* buffers, u8 num_buffers, const u64* offsets = null);
		void render_command_set_ibuffer(const index_buffer_t& buffer, const u64& offset = 0);
		void render_command_set_rt(const render_target_t& rt_id, const render_target_t& depth_id = no_render_target);
		void render_command_set_rts(const render_target_t* render_targets, u8 num_rts, const render_target_t& depth_id = no_render_target);
		void render_command_set_tex2d(const u8& tex_slot, const texture_2d_t& tex_id);
		void render_command_set_tex3d(const u8& tex_slot, const texture_3d_t& tex_id);
		void render_command_set_texcube(const u8& tex_slot, const texture_cube_t& tex_id);
		void render_command_set_texarray(const u8& tex_slot, const texture_array_t& tex_id);
		void render_command_set_viewport(const math::urect& rect);
		void render_command_set_topology(const primitive_topology& topology);
		void render_command_set_cull_mode(const cull_mode& cull);
		void render_command_set_vertex_elements(const u32& vertex_elements);
		void render_command_set_shader(const shader_type type, const shader_t& shader_id);
		void render_command_set_depth_enabled(const bool& enabled);
		void render_command_set_wireframe(const bool& enabled);
	}
}