#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <rengine/math/math.h>

namespace rengine {
    namespace graphics {
        struct vertex {
            math::vec3 point;
            math::byte_color color{ math::byte_color::white };
        };
        struct vertex_uv : vertex {
            math::vec2 uv;
        };

        struct line_t {
            vertex a;
            vertex b;
        };

        struct triangle {
            vertex a;
            vertex b;
            vertex c;
        };

        struct quad {
            math::vec3 center;
            math::color color;
            math::vec2 size;
        };

        struct cube {
            math::vec3 center;
            math::vec3 size;
            math::color color;
        };

        struct geometry {
            math::vec3* vertices;
            math::color* colors;
            math::vec2* texcoords;
            u32* indices;
            u32 num_vertices;
            u32 num_indices;
        };

        R_EXPORT void renderer_begin_draw();
        R_EXPORT void renderer_end_draw();
        R_EXPORT void renderer_set_vertex_color(const math::byte_color& color);
        R_EXPORT void renderer_add_point(const vertex& vertex);
        R_EXPORT void renderer_add_line(const line_t& line);
        R_EXPORT void renderer_add_line(const math::vec3& a, const math::vec3& b);
        R_EXPORT void renderer_add_triangle(const triangle& value);
        R_EXPORT void renderer_add_triangle(const math::vec3& a, const math::vec3& b, const math::vec3& c);
        R_EXPORT void renderer_add_quad(const quad& quad);
        R_EXPORT void renderer_add_quad(const math::vec3& center, const math::vec2& size);
        R_EXPORT void renderer_add_cube(const cube& cube);
        R_EXPORT void renderer_add_cube(const math::vec3& center, const math::vec3& size);
        R_EXPORT void renderer_add_geometry(const geometry& geometry);
    }
}