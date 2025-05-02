#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <rengine/math/math.h>

namespace rengine {
    namespace graphics {
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
        R_EXPORT void renderer_push_vertex(const math::vec3& point);
		R_EXPORT void renderer_set_uv(const math::vec2& uv);
        R_EXPORT void renderer_set_color(const math::byte_color& color);
        R_EXPORT void renderer_draw_point();
        R_EXPORT void renderer_draw_line();
        R_EXPORT void renderer_draw_triangle();
        R_EXPORT void renderer_add_quad(const quad& quad);
        R_EXPORT void renderer_add_quad(const math::vec3& center, const math::vec2& size);
        R_EXPORT void renderer_add_cube(const cube& cube);
        R_EXPORT void renderer_add_cube(const math::vec3& center, const math::vec3& size);
        R_EXPORT void renderer_add_geometry(const geometry& geometry);
    }
}