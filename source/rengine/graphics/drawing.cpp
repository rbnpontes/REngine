#include "./drawing.h"
#include "./drawing_private.h"

#include "../exceptions.h"
#include "../core/vector_utils_private.h"

#include <stb/stb_easy_font.h>

namespace rengine {
	namespace graphics {
		void renderer_begin_draw()
		{
			auto& state = g_drawing_state;
			state.vertex_queue.clear();
			state.triangles.clear();
			state.lines.clear();
			state.points.clear();
			state.text_quads.clear();
			state.current_color = math::byte_color::white;
			state.current_transform = {};
		}

		void renderer_end_draw()
		{
			drawing__check_buffer_requirements();
			drawing__upload_buffers();
			drawing__submit_draw_calls();
		}

		void renderer_push_vertex(const math::vec3& point)
		{
			auto& state = g_drawing_state;
			drawing__compute_transform();
			
			vertex_uv_data vertex;
			vertex.point = math::matrix4x4::mul(state.current_transform.transform, point);
			vertex.color = state.current_color;
			vertex.uv = state.current_uv;
			g_drawing_state.vertex_queue.push(vertex);
		}

		void renderer_set_uv(const math::vec2& uv)
		{
			g_drawing_state.current_uv = uv;
		}

		void renderer_set_color(const math::byte_color& color)
		{
			g_drawing_state.current_color = color;
		}

		void renderer_set_transform(const math::matrix4x4& transform)
		{
			g_drawing_state.current_transform.transform = transform;
			g_drawing_state.current_transform.dirty = false;
		}

		void renderer_translate(const math::vec3& translation)
		{
			auto& state = g_drawing_state;
			state.current_transform.position = translation;
			state.current_transform.dirty = true;
		}

		void renderer_rotate(number_t degree)
		{
			renderer_rotate(math::quat::from_rotation(degree));
		}

		void renderer_rotate(const math::quat& rotation)
		{
			auto& state = g_drawing_state;
			state.current_transform.rotation = rotation;
			state.current_transform.dirty = true;
		}

		void renderer_scale(number_t scale)
		{
			renderer_scale(math::vec2(scale, scale));
		}

		void renderer_scale(const math::vec2& scale)
		{
			auto& state = g_drawing_state;
			state.current_transform.scale = scale;
			state.current_transform.dirty = true;
		}

		void renderer_draw_point()
		{
			if (!drawing__assert_vert_count(1))
				return;

			const auto& vertex = g_drawing_state.vertex_queue.front();
			g_drawing_state.vertex_queue.pop();

			g_drawing_state.points.push_back({
				vertex.point,
				vertex.color
			});
		}

		void renderer_draw_line()
		{
			if (!drawing__assert_vert_count(2))
				return;

			const auto& a = g_drawing_state.vertex_queue.front();
			g_drawing_state.vertex_queue.pop();
			const auto& b = g_drawing_state.vertex_queue.front();
			g_drawing_state.vertex_queue.pop();

			g_drawing_state.lines.push_back({
				{ a.point, a.color },
				{ b.point, b.color }
			});
		}

		void renderer_draw_triangle()
		{
			if (!drawing__assert_vert_count(3))
				return;

			const auto& a = g_drawing_state.vertex_queue.front();
			g_drawing_state.vertex_queue.pop();
			const auto& b = g_drawing_state.vertex_queue.front();
			g_drawing_state.vertex_queue.pop();
			const auto& c = g_drawing_state.vertex_queue.front();
			g_drawing_state.vertex_queue.pop();

			g_drawing_state.triangles.push_back({
				a, b, c
			});
		}

		void renderer_draw_quad(const math::vec3& center, const math::vec2& size)
		{
			number_t half_data[4];
			// fast divide size by half (2)
			sse_store_number(
				half_data,
				sse_mul_number(
					sse_set_number(0, 0, size.x, size.y),
					sse_set_single_number(0.5f)
				)
			);


			math::vec3 left_top = center + math::vec3(-half_data[0], half_data[1]);
			math::vec3 right_top = center + math::vec3(half_data[0], half_data[1]);
			math::vec3 left_bottom = center + math::vec3(-half_data[0], -half_data[1]);
			math::vec3 right_bottom = center + math::vec3(half_data[0], -half_data[1]);

			renderer_set_uv({ 0., 1. });
			renderer_push_vertex(left_top);
			
			renderer_set_uv({ 1., 1. });
			renderer_push_vertex(right_top);
			
			renderer_set_uv({ 1., 0. });
			renderer_push_vertex(right_bottom);
			renderer_draw_triangle();

			renderer_set_uv({ 0., 1. });
			renderer_push_vertex(left_top);

			renderer_set_uv({ 1., 0. });
			renderer_push_vertex(right_bottom);

			renderer_set_uv({});
			renderer_push_vertex(left_bottom);
			renderer_draw_triangle();
		}

		void renderer_draw_quad_lines(const math::vec3& center, const math::vec2& size)
		{
		}

		void renderer_draw_text(c_str text)
		{
			auto& state = g_drawing_state;
			u32 len = strlen(text);
			size_t offset = state.text_quads.size();
			state.text_quads.resize(len * 4);

			u32 num_quads = stb_easy_font_print(0, 0,
				const_cast<char*>(text),
				reinterpret_cast<byte*>(&g_drawing_state.current_color.r),
				state.text_quads.data() + (offset * sizeof(vertex_data)),
				len * (sizeof(vertex_data) * 4));
		}

		void renderer_add_cube(const cube& cube)
		{
			throw not_implemented_exception();
		}

		void renderer_add_cube(const math::vec3& center, const math::vec3& size)
		{
			throw not_implemented_exception();
		}

		void renderer_add_geometry(const geometry& geometry)
		{
			throw not_implemented_exception();
		}
	}
}