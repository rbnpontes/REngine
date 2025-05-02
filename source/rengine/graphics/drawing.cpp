#include "./drawing.h"
#include "./drawing_private.h"

#include "../exceptions.h"
#include "../core/vector_utils_private.h"

namespace rengine {
	namespace graphics {
		void renderer_begin_draw()
		{
			auto& state = g_drawing_state;
			state.vertex_queue.clear();
			state.triangles.clear();
			state.lines.clear();
			state.points.clear();
			state.current_color = math::byte_color::white;
		}

		void renderer_end_draw()
		{
			drawing__check_buffer_requirements();
			drawing__upload_buffers();
			drawing__submit_draw_calls();
		}

		void renderer_push_vertex(const math::vec3& point)
		{
			vertex_uv_data vertex = { point, g_drawing_state.current_color, g_drawing_state.current_uv };
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

		void renderer_add_quad(const quad& quad)
		{
			throw not_implemented_exception();
		}

		void renderer_add_quad(const math::vec3& center, const math::vec2& size)
		{
			throw not_implemented_exception();
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