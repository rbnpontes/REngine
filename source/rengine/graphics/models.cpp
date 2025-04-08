#include "./models.h"
#include "./models_private.h"

#include "../exceptions.h"

namespace rengine {
	namespace graphics {
		void renderer_begin_draw()
		{
			throw not_implemented_exception();
		}
		
		void renderer_end_draw()
		{
			throw not_implemented_exception();
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

		void renderer_add_triangle(const triangle& triangle)
		{
			throw not_implemented_exception();
		}

		void renderer_add_triangle(const math::vec3& a, const math::vec3& b, const math::vec3& c)
		{
			throw not_implemented_exception();
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