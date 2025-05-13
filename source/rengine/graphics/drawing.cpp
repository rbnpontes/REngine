#include "./drawing.h"
#include "./drawing_private.h"

#include "../exceptions.h"
#include "../core/vector_utils_private.h"

#include <stb/stb_easy_font.h>

namespace rengine {
	namespace graphics {
		void drawing_begin_draw()
		{
			drawing__begin_draw();
		}

		void drawing_end_draw()
		{
			drawing__end_draw();
		}

		void drawing_push_vertex(const math::vec3& point)
		{
			auto& state = g_drawing_state;
			drawing__compute_transform();
			
			vertex_uv_data vertex;
			vertex.point = math::matrix4x4::mul(state.current_transform.transform, point);
			vertex.color = state.current_color;
			vertex.uv = state.current_uv;
			g_drawing_state.vertex_queue.push(vertex);
		}

		void drawing_set_uv(const math::vec2& uv)
		{
			g_drawing_state.current_uv = uv;
		}

		void drawing_set_color(const math::byte_color& color)
		{
			g_drawing_state.current_color = color;
		}

		void drawing_set_transform(const math::matrix4x4& transform)
		{
			g_drawing_state.current_transform.transform = transform;
			g_drawing_state.current_transform.dirty = false;
		}

		void drawing_translate(const math::vec3& translation)
		{
			auto& state = g_drawing_state;
			state.current_transform.position = translation;
			state.current_transform.dirty = true;
		}

		void drawing_rotate(number_t degree)
		{
			drawing_rotate(math::quat::from_rotation(degree));
		}

		void drawing_rotate(const math::quat& rotation)
		{
			auto& state = g_drawing_state;
			state.current_transform.rotation = rotation;
			state.current_transform.dirty = true;
		}

		void drawing_scale(number_t scale)
		{
			drawing_scale(math::vec2(scale, scale));
		}

		void drawing_scale(const math::vec2& scale)
		{
			auto& state = g_drawing_state;
			state.current_transform.scale = scale;
			state.current_transform.dirty = true;
		}

		void drawing_reset_transform()
		{
			g_drawing_state.current_transform = {};
		}

		void drawing_draw_point()
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

		void drawing_draw_line()
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

		void drawing_draw_triangle()
		{
			if (!drawing__assert_vert_count(3))
				return;

			const auto& a = g_drawing_state.vertex_queue.front();
			g_drawing_state.vertex_queue.pop();
			const auto& b = g_drawing_state.vertex_queue.front();
			g_drawing_state.vertex_queue.pop();
			const auto& c = g_drawing_state.vertex_queue.front();
			g_drawing_state.vertex_queue.pop();

			triangle_data triangle;
			auto reverse = g_drawing_state.triangles.size() > 0;

			if (reverse)
				triangle = { a, b, c };
			else
				triangle = { c, b, a };

			g_drawing_state.triangles.push_back(triangle);
		}

		void drawing_draw_quad(const math::vec3& center, const math::vec2& size)
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


			math::vec3 left_top = center + math::vec3(-half_data[0], -half_data[1]);
			math::vec3 right_top = center + math::vec3(half_data[0], -half_data[1]);
			math::vec3 left_bottom = center + math::vec3(-half_data[0], half_data[1]);
			math::vec3 right_bottom = center + math::vec3(half_data[0], half_data[1]);

			drawing_draw_rect(left_top, right_top, right_bottom, left_bottom);
		}

		void drawing_draw_rect(const math::vec3& left_top, const math::vec3& right_top, const math::vec3& right_bottom, const math::vec3& left_bottom)
		{
			drawing_set_uv({ 0., 1. });
			drawing_push_vertex(left_bottom);

			drawing_set_uv({ 0., 0. });
			drawing_push_vertex(left_top);

			drawing_set_uv({ 1., 0. });
			drawing_push_vertex(right_top);
			drawing_draw_triangle();

			drawing_set_uv({ 1., 0. });
			drawing_push_vertex(right_top);

			drawing_set_uv({ 1., 1. });
			drawing_push_vertex(right_bottom);

			drawing_set_uv({ 0., 1.});
			drawing_push_vertex(left_bottom);
			drawing_draw_triangle();
		}

		void drawing_draw_quad_lines(const math::vec3& center, const math::vec2& size)
		{
		}

		void drawing_draw_text(c_str text)
		{
			if (!text)
				return;
			const auto len = strlen(text);
			if (len > DRAWING_MAX_TEXT_LENGTH)
				throw graphics_exception(
					fmt::format(
						strings::exceptions::g_drawing_exceed_text_len,
						DRAWING_MAX_TEXT_LENGTH, len
					).c_str()
				);

			auto& state = g_drawing_state;
			u32 buff_size = stb_easy_font_calc_buf_size(const_cast<char*>(text));

			const auto required_size = buff_size / sizeof(vertex_data);
			static vector<vertex_data> tmp_vertices;
			tmp_vertices.resize(required_size);

			u32 num_quads = stb_easy_font_print(0, 0,
				const_cast<char*>(text),
				null,
				tmp_vertices.data(),
				required_size * sizeof(vertex_data));

			for (u32 i = 0; i < required_size; i += 4) {
				vertex_data left_top = tmp_vertices[i + 0];
				vertex_data right_top = tmp_vertices[i + 1];
				vertex_data right_bottom = tmp_vertices[i + 2];
				vertex_data left_bottom = tmp_vertices[i + 3];

				drawing_draw_rect(left_top.point, right_top.point, right_bottom.point, left_bottom.point);
			}

			tmp_vertices.reset();
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