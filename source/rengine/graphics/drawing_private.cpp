#include "./drawing_private.h"
#include "./buffer_manager.h"
#include "./pipeline_state_manager.h"
#include "./shader_manager.h"
#include "./graphics_private.h"
#include "./graphics.h"

#include "../strings.h"
#include "../exceptions.h"

#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		drawing_state g_drawing_state = {};

		void drawing__init()
		{
			drawing__require_vbuffer_size(DRAWING_DEFAULT_VBUFFER_SIZE);
			drawing__require_ibuffer_size(DRAWING_DEFAULT_IBUFFER_SIZE);
			drawing__prewarm_pipelines();
		}

		void drawing__deinit() {
			auto& state = g_drawing_state;

			buffer_mgr_vbuffer_free(state.vertex_buffer);
			buffer_mgr_ibuffer_free(state.index_buffer);
			buffer_mgr_cbuffer_free(state.constant_buffer);

			state.vertex_buffer = no_vertex_buffer;
			state.index_buffer = no_index_buffer;
			state.constant_buffer = no_constant_buffer;
		}

		void drawing__require_vbuffer_size(u32 buffer_size)
		{
			auto& state = g_drawing_state;
			try {
				buffer_size += DRAWING_DEFAULT_VBUFFER_SIZE;
				if (state.vertex_buffer == no_vertex_buffer)
					state.vertex_buffer = buffer_mgr_vbuffer_create({
						strings::graphics::g_drawing_vbuffer_name,
						buffer_size,
						null,
						true
						});
				else if (buffer_size > state.vertex_buffer_size)
					state.vertex_buffer = buffer_mgr_vbuffer_realloc(state.vertex_buffer, buffer_size);

				state.vertex_buffer_size = buffer_size;
			}
			catch (const graphics_exception& exception) {
				throw graphics_exception(
					fmt::format(strings::exceptions::g_models_failed_to_alloc_vbuffer, buffer_size).c_str()
				);
			}
		}

		void drawing__require_ibuffer_size(u32 buffer_size)
		{
			auto& state = g_drawing_state;
			try {
				buffer_size += DRAWING_DEFAULT_IBUFFER_SIZE;
				if (state.index_buffer == no_index_buffer)
					state.index_buffer = buffer_mgr_ibuffer_create({
						strings::graphics::g_drawing_ibuffer_name,
						buffer_size,
						null,
						true
						});
				else if (buffer_size > state.index_buffer_size)
					state.index_buffer = buffer_mgr_ibuffer_realloc(state.index_buffer, buffer_size);

				state.index_buffer_size = buffer_size;
			}
			catch (const graphics_exception& exception) {
				throw graphics_exception(
					fmt::format(strings::exceptions::g_models_failed_to_alloc_ibuffer, buffer_size).c_str()
				);
			}
		}

		void drawing__prewarm_pipelines()
		{
			// prewarm shaders
			shader_create_desc shader_desc = {};
			shader_desc.name = strings::graphics::g_drawing_vshader_name;
			shader_desc.type = shader_type::vertex;
			shader_desc.source_code = strings::graphics::shaders::g_model_nouv_vertex;
			shader_desc.source_code_length = strlen(shader_desc.source_code);
			const auto& vertex = shader_mgr_create(shader_desc);

			shader_desc.name = strings::graphics::g_drawing_pshader_name;
			shader_desc.type = shader_type::pixel;
			shader_desc.source_code = strings::graphics::shaders::g_model_nouv_pixel;
			shader_desc.source_code_length = strlen(shader_desc.source_code);

			const auto& pixel = shader_mgr_create(shader_desc);

			graphics_pipeline_state_create create_desc = {};
			create_desc.name = strings::graphics::g_drawing_pipeline_name;
			create_desc.vertex_shader = vertex;
			create_desc.pixel_shader = pixel;
			create_desc.topology = primitive_topology::triangle_list;
			create_desc.num_render_targets = 1;
			create_desc.render_target_formats[0] = get_default_backbuffer_format();
			create_desc.depth_stencil_format = get_default_depthbuffer_format();
			create_desc.vertex_elements = (u32)vertex_elements::position | (u32)vertex_elements::color;

			for (u32 i = 0; i < (u8)primitive_topology::line_strip; ++i) {
				create_desc.topology = (primitive_topology)i;
				create_desc.wireframe = false;
				pipeline_state_mgr_create_graphics(create_desc);
				create_desc.wireframe = true;
				pipeline_state_mgr_create_graphics(create_desc);
			}

			g_drawing_state.triangle_vs_shader = vertex;
			g_drawing_state.triangle_ps_shader = pixel;
		}

		void drawing__upload_buffers()
		{
			auto& state = g_drawing_state;
			u64 cpy_size = 0;
			ptr data = buffer_mgr_vbuffer_map(state.vertex_buffer, buffer_map_type::write);

			if (state.num_triangles > 0) {
				cpy_size = state.num_triangles * sizeof(triangle);
				memcpy(data, state.triangles.data(), cpy_size);
				data = static_cast<u8*>(data) + cpy_size;
			}

			if (state.num_lines > 0) {
				cpy_size = state.num_lines * sizeof(line_t);
				memcpy(data, state.lines.data(), cpy_size);
				data = static_cast<u8*>(data) + cpy_size;
			}

			buffer_mgr_vbuffer_unmap(state.vertex_buffer);
		}

		void drawing__submit_draw_calls()
		{
			if (g_drawing_state.num_triangles > 0)
				drawing__draw_triangles();
			if (g_drawing_state.num_lines > 0)
				drawing__draw_lines();
		}

		void drawing__draw_triangles()
		{
			const auto& state = g_drawing_state;
			renderer_set_vbuffer(g_drawing_state.vertex_buffer, 0);
			renderer_set_topology(primitive_topology::triangle_strip);
			renderer_set_depth_enabled(true);
			renderer_set_wireframe(false);
			renderer_set_cull_mode(cull_mode::clock_wise);
			renderer_set_vertex_shader(g_drawing_state.triangle_vs_shader);
			renderer_set_pixel_shader(g_drawing_state.triangle_ps_shader);
			renderer_set_vertex_elements((u32)vertex_elements::position | (u32)vertex_elements::color);
			renderer_draw({ state.num_triangles * 3 });
		}

		void drawing__draw_lines()
		{
			const auto& state = g_drawing_state;
			renderer_set_vbuffer(state.vertex_buffer, state.num_triangles * sizeof(triangle));
			renderer_set_topology(primitive_topology::line_strip);
			renderer_set_depth_enabled(true);
			renderer_set_wireframe(false);
			renderer_set_cull_mode(cull_mode::clock_wise);
			renderer_set_vertex_shader(g_drawing_state.triangle_vs_shader);
			renderer_set_pixel_shader(g_drawing_state.triangle_ps_shader);
			renderer_set_vertex_elements((u32)vertex_elements::position | (u32)vertex_elements::color);
			renderer_draw({ g_drawing_state.num_lines * 2 });
		}
	}
}