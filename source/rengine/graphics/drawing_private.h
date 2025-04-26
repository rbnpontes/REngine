#pragma once
#include "../base_private.h"
#include "./drawing.h"

#include <GraphicsTypes.h>
#include <Buffer.h>
#include <PipelineState.h>

namespace rengine {
    namespace graphics {
		struct drawing_state
		{
			vector<vertex> points;
			vector<triangle> triangles;
			vector<line_t> lines;
			vector<vertex_uv> geometry_vertices;
			vector<u32> geometry_indices;

			math::byte_color current_vertex_color{ math::byte_color::white };
			u32 num_triangles;
			u32 num_lines;
			u32 num_vertices;
			u32 num_indices;

			vertex_buffer_t vertex_buffer{ no_vertex_buffer };
			index_buffer_t index_buffer{ no_index_buffer };
			constant_buffer_t constant_buffer{ no_constant_buffer };
			shader_t triangle_vs_shader{ no_shader };
			shader_t triangle_ps_shader{ no_shader };

			u32 vertex_buffer_size{ 0 };
			u32 index_buffer_size{ 0 };

			array<u32, 3> offsets;
		};
		extern drawing_state g_drawing_state;

		void drawing__init();
		void drawing__deinit();
		void drawing__require_vbuffer_size(u32 buffer_size);
		void drawing__require_ibuffer_size(u32 buffer_size);
		void drawing__prewarm_pipelines();
		void drawing__upload_buffers();
		void drawing__submit_draw_calls();
		void drawing__draw_triangles();
		void drawing__draw_lines();
    }
}