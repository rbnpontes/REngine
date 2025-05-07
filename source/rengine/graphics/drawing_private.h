#pragma once
#include "../base_private.h"
#include "./drawing.h"

#include "../core/pool.h"
#include "../io/logger.h"

#include <GraphicsTypes.h>
#include <Buffer.h>
#include <PipelineState.h>

namespace rengine {
    namespace graphics {
		struct vertex_data {
			math::vec3 point;
			math::byte_color color{ math::byte_color::white };
		};

		struct vertex_uv_data : vertex_data {
			math::vec2 uv;
		};

		struct triangle_data {
			vertex_uv_data a;
			vertex_uv_data b;
			vertex_uv_data c;
		};

		struct line_data {
			vertex_data a;
			vertex_data b;
		};

		struct drawing_transform {
			math::matrix4x4 transform{ math::matrix4x4::identity };
			math::vec3 position{ math::vec3::zero };
			math::quat rotation{};
			math::vec2 scale{ math::vec2::one };
			bool dirty{ false };
		};

		struct drawing_state
		{
			io::ILog* log;
			core::fixed_queue<vertex_uv_data, 3, u8> vertex_queue;

			vector<vertex_data> points;
			vector<triangle_data> triangles;
			vector<line_data> lines;

			math::byte_color current_color{ math::byte_color::white };
			math::vec2 current_uv{ 0, 0 };
			drawing_transform current_transform{};

			vertex_buffer_t vertex_buffer{ no_vertex_buffer };
			constant_buffer_t constant_buffer{ no_constant_buffer };

			shader_t vertex_shader[2]{ no_shader };
			shader_t pixel_shader{ no_shader };

			u32 vertex_buffer_size{ 0 };
		};
		extern drawing_state g_drawing_state;

		void drawing__init();
		void drawing__deinit();
		bool drawing__assert_vert_count(u32 count);
		void drawing__require_vbuffer_size(u32 buffer_size);
		void drawing__compile_shaders();
		void drawing__prewarm_pipelines();
		void drawing__check_buffer_requirements();
		void drawing__upload_buffers();
		void drawing__submit_draw_calls();
		void drawing__draw_triangles();
		void drawing__draw_lines();
		void drawing__draw_points();
		void drawing__compute_transform();

		void drawing__begin_draw();
		void drawing__end_draw();
    }
}