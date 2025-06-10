#include "./drawing_private.h"
#include "./buffer_manager.h"
#include "./pipeline_state_manager.h"
#include "./shader_manager.h"
#include "./graphics_private.h"
#include "./graphics.h"

#include "../rengine_private.h"
#include "../core/profiler.h"

#include "../strings.h"
#include "../exceptions.h"

#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		drawing_state g_drawing_state = {};

		void drawing__init()
		{
			g_drawing_state.log = io::logger_use(strings::logs::g_drawing_cmd_tag);

			size_t size = DRAWING_DEFAULT_TRIANGLE_COUNT * sizeof(triangle_data);
			size += DRAWING_DEFAULT_LINES_COUNT * sizeof(line_data);
			size += DRAWING_DEFAULT_POINTS_COUNT * sizeof(vertex_data);	
			drawing__require_vbuffer_size(size);
			drawing__compile_shaders();
			drawing__prewarm_pipelines();
		}

		void drawing__deinit() {
			auto& state = g_drawing_state;

			buffer_mgr_vbuffer_free(state.vertex_buffer);

			state.vertex_buffer = no_vertex_buffer;
			state.constant_buffer = no_constant_buffer;
		}

		bool drawing__assert_vert_count(u32 count)
		{
			const auto log = g_drawing_state.log;
			bool result = true;

			if (g_drawing_state.vertex_queue.size() < count) {
				result = false;
				log->error(
					fmt::format(strings::logs::g_draw_require_x_vertices, count, g_drawing_state.vertex_queue.size()).c_str()
				);
			}

			return result;
		}

		void drawing__require_vbuffer_size(u32 buffer_size)
		{
			profile();
			auto& state = g_drawing_state;
			size_t additional_size = DRAWING_DEFAULT_TRIANGLE_COUNT * sizeof(triangle_data);
			additional_size += DRAWING_DEFAULT_LINES_COUNT * sizeof(line_data);
			additional_size += DRAWING_DEFAULT_POINTS_COUNT * sizeof(vertex_data);
			if (buffer_size > additional_size)
				buffer_size += additional_size;

			try {
				state.vertex_buffer = buffer_mgr_get_dynamic_vbuffer(buffer_size);
			}
			catch (const graphics_exception& exception) {
				throw graphics_exception(
					fmt::format(strings::exceptions::g_drawing_failed_to_alloc_vbuffer, buffer_size).c_str()
				);
			}
		}

		/*void drawing__require_ibuffer_size(u32 buffer_size)
		{
			auto& state = g_drawing_state;
			try {
				if (state.index_buffer == no_vertex_buffer)
					state.index_buffer = buffer_mgr_ibuffer_create({
						strings::graphics::g_drawing_ibuffer_name,
						buffer_size,
						null,
						true,
						});
				else if (buffer_size > state.index_buffer_size)
			}
			catch (const graphics_exception& exception) {
				throw graphics_exception(
					fmt::format(strings::exceptions::g_models_failed_to_alloc_ibuffer, buffer_size).c_str()
				);
			}
		}*/

		void drawing__compile_shaders()
		{
			auto& state = g_drawing_state;

                        shader_create_desc shader_desc = {};
                        shader_t vertex_shader[2]{ no_shader };
                        shader_t pixel_shader{ no_shader };

                        shader_desc.name = strings::graphics::g_drawing_vshader_name;
                        shader_desc.type = shader_type::vertex;
                        shader_desc.source_code = strings::graphics::shaders::g_drawing_vs;
                        shader_desc.source_code_length = strlen(shader_desc.source_code);
                        shader_desc.vertex_elements = (u32)vertex_elements::position | (u32)vertex_elements::color;

                        vertex_shader[0] = shader_mgr_create(shader_desc);
                        shader_desc.vertex_elements |= (u32)vertex_elements::uv;
                        vertex_shader[1] = shader_mgr_create(shader_desc);

                        shader_desc.num_macros = 0;
                        shader_desc.name = strings::graphics::g_drawing_pshader_name;
                        shader_desc.type = shader_type::pixel;
                        shader_desc.source_code = strings::graphics::shaders::g_drawing_ps;
                        shader_desc.source_code_length = strlen(shader_desc.source_code);

                        pixel_shader = shader_mgr_create(shader_desc);

                        shader_program_create_desc program_desc{};
                        program_desc.desc.pixel_shader = pixel_shader;
                        program_desc.desc.vertex_shader = vertex_shader[0];
                        state.program[0] = shader_mgr_create_program(program_desc);

                        program_desc.desc.vertex_shader = vertex_shader[1];
                        state.program[1] = shader_mgr_create_program(program_desc);
                }

		void drawing__prewarm_pipelines()
		{
                        graphics_pipeline_state_create create_desc = {};
                        create_desc.name = strings::graphics::g_drawing_pipeline_name;
                        create_desc.topology = primitive_topology::triangle_list;
                        create_desc.num_render_targets = 1;
                        create_desc.render_target_formats[0] = get_default_backbuffer_format();
                        create_desc.depth_stencil_format = get_default_depthbuffer_format();
                        create_desc.vertex_elements = (u32)vertex_elements::position | (u32)vertex_elements::color;

                        for (u32 i = 0; i < 2; ++i) {
                                create_desc.shader_program = g_drawing_state.program[i];
                                for (u32 j = 0; j < (u8)primitive_topology::line_strip; ++j) {
                                        create_desc.topology = (primitive_topology)j;
                                        create_desc.wireframe = false;
                                        pipeline_state_mgr_create_graphics(create_desc);
                                        create_desc.wireframe = true;
                                        pipeline_state_mgr_create_graphics(create_desc);
                                }
                        }
		}

		void drawing__check_buffer_requirements()
		{
			profile();
			const auto& state = g_drawing_state;
			size_t required_size = state.triangles.size() * sizeof(triangle_data);
			required_size += state.lines.size() * sizeof(line_data);
			required_size += state.points.size() * sizeof(vertex_data);

			drawing__require_vbuffer_size(required_size);
		}

		void drawing__upload_buffers()
		{
			profile();

			auto& state = g_drawing_state;
			u64 cpy_size = 0;
			ptr data = buffer_mgr_vbuffer_map(state.vertex_buffer, buffer_map_type::write);

			if (state.triangles.size() > 0) {
				cpy_size = state.triangles.size() * sizeof(triangle_data);
				memcpy(data, state.triangles.data(), cpy_size);
				data = static_cast<u8*>(data) + cpy_size;
			}

			if (state.lines.size() > 0) {
				cpy_size = state.lines.size() * sizeof(line_data);
				memcpy(data, state.lines.data(), cpy_size);
				data = static_cast<u8*>(data) + cpy_size;
			}

			if (state.points.size() > 0) {
				cpy_size = state.points.size() * sizeof(vertex_data);
				memcpy(data, state.points.data(), cpy_size);
				data = static_cast<u8*>(data) + cpy_size;
			}

			buffer_mgr_vbuffer_unmap(state.vertex_buffer);
		}

		void drawing__submit_draw_calls()
		{
			if (g_drawing_state.triangles.size() > 0)
				drawing__draw_triangles();
			if (g_drawing_state.lines.size() > 0)
				drawing__draw_lines();
			if (g_drawing_state.points.size() > 0)
				drawing__draw_points();
		}

		void drawing__draw_triangles()
		{
			profile();

			const auto& state = g_drawing_state;
			renderer_set_vbuffer(g_drawing_state.vertex_buffer, 0);
			renderer_set_topology(primitive_topology::triangle_list);
			renderer_set_depth_enabled(true);
			renderer_set_wireframe(false);
			renderer_set_cull_mode(cull_mode::clock_wise);
                        renderer_set_program(g_drawing_state.program[1]);
                        renderer_set_vertex_elements((u32)vertex_elements::position | (u32)vertex_elements::color | (u32)vertex_elements::uv);
                        renderer_draw({ (u32)state.triangles.size() * 3 });
		}

		void drawing__draw_lines()
		{
			profile();

			const auto& state = g_drawing_state;
			renderer_set_vbuffer(state.vertex_buffer, state.triangles.size() * sizeof(triangle_data));
			renderer_set_topology(primitive_topology::line_strip);
			renderer_set_depth_enabled(true);
			renderer_set_wireframe(false);
			renderer_set_cull_mode(cull_mode::clock_wise);
                        renderer_set_program(g_drawing_state.program[0]);
                        renderer_set_vertex_elements((u32)vertex_elements::position | (u32)vertex_elements::color);
                        renderer_draw({ (u32)g_drawing_state.lines.size() * 2 });
		}

		void drawing__draw_points()
		{
			profile();

			const auto& state = g_drawing_state;
			u32 offset = state.triangles.size() * sizeof(triangle_data);
			offset += state.lines.size() * sizeof(line_data);

			renderer_set_vbuffer(state.vertex_buffer, offset);
			renderer_set_topology(primitive_topology::point_list);
			renderer_set_depth_enabled(true);
			renderer_set_wireframe(false);
			renderer_set_cull_mode(cull_mode::clock_wise);
                        renderer_set_program(g_drawing_state.program[0]);
                        renderer_set_vertex_elements((u32)vertex_elements::position | (u32)vertex_elements::color);
                        renderer_draw({ (u32)g_drawing_state.points.size() });
		}

		void drawing__compute_transform()
		{
			auto& state = g_drawing_state;
			if (!state.current_transform.dirty)
				return;

			state.current_transform.dirty = false;
			state.current_transform.transform = math::matrix4x4::transform(state.current_transform.position,
				state.current_transform.rotation,
				math::vec3(state.current_transform.scale.x, state.current_transform.scale.y, 1));
		}

		void drawing__begin_draw()
		{
			profile_begin_name(nameof(drawing));

			auto& state = g_drawing_state;
			state.vertex_queue.clear();
			state.triangles.reset();
			state.lines.reset();
			state.points.reset();
			state.current_color = math::byte_color::white;
			state.current_transform = {};
		}

		void drawing__end_draw()
		{
			drawing__check_buffer_requirements();
			drawing__upload_buffers();
			drawing__submit_draw_calls();

			profile_end();
		}
	}
}