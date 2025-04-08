#include "./models.h"
#include "./models_private.h"

#include "../exceptions.h"
#include "../core/vector_utils_private.h"

namespace rengine {
	namespace graphics {
		void renderer_begin_draw()
		{
			auto& state = g_models_state;
			state.num_triangles = state.num_lines = state.num_vertices = state.num_indices = 0;
			state.current_vertex_color = math::byte_color::white;
			state.offsets.fill(0);
		}
		
		void renderer_end_draw()
		{
			models__upload_buffers();
			models__submit_draw_calls();
		}
		
		void renderer_set_vertex_color(const math::byte_color& color)
		{
			throw not_implemented_exception();
		}
		
		void renderer_add_point(const vertex& vertex)
		{
			throw not_implemented_exception();
		}
		
		void renderer_add_line(const line& line)
		{
			throw not_implemented_exception();
		}

		void renderer_add_line(const math::vec3& a, const math::vec3& b)
		{
			throw not_implemented_exception();
		}

		void renderer_add_triangle(const triangle& value)
		{
			core::vector_utils_insert_item<triangle>(g_models_state.triangles, value, &g_models_state.num_triangles);
		}

		void renderer_add_triangle(const math::vec3& a, const math::vec3& b, const math::vec3& c)
		{
			triangle value = {};
			value.a.point = a;
			value.b.point = b;
			value.c.point = c;
			value.a.color = value.b.color = value.c.color = g_models_state.current_vertex_color;

			renderer_add_triangle(value);
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