#pragma once
#include "../base_private.h"
#include "./models.h"

#include <GraphicsTypes.h>
#include <Buffer.h>
#include <PipelineState.h>

namespace rengine {
	namespace graphics {
		struct models_state
		{
			vector<vertex> points;
			vector<triangle> triangles;
			vector<line> lines;
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
		};
		extern models_state g_models_state;
		
		void models__init();
		void models__deinit();
		void models__require_vbuffer_size(u32 buffer_size);
		void models__require_ibuffer_size(u32 buffer_size);
		void models__prewarm_pipelines();
	}
}