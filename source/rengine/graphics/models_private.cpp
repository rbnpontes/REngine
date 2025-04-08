#include "./models_private.h"
#include "./buffer_manager.h"
#include "./pipeline_state_manager.h"
#include "./shader_manager.h"
#include "./graphics_private.h"
#include "./graphics.h"

#include "../strings.h"
#include "../exceptions.h"

#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		models_state g_models_state = {};
		void models__init()
		{
			models__require_vbuffer_size(MODELS_DEFAULT_VBUFFER_SIZE);
			models__require_ibuffer_size(MODELS_DEFAULT_IBUFFER_SIZE);
		}

		void models__deinit() {
			auto& state = g_models_state;

			buffer_mgr_vbuffer_free(state.vertex_buffer);
			buffer_mgr_ibuffer_free(state.index_buffer);
			buffer_mgr_cbuffer_free(state.constant_buffer);

			state.vertex_buffer = no_vertex_buffer;
			state.index_buffer = no_index_buffer;
			state.constant_buffer = no_constant_buffer;
		}

		void models__require_vbuffer_size(u32 buffer_size)
		{
			auto& state = g_models_state;
			try {
				if (state.vertex_buffer == no_vertex_buffer)
					state.vertex_buffer = buffer_mgr_vbuffer_create({
						strings::graphics::g_models_vbuffer_name,
						buffer_size,
						null,
						true
					});
				else
					buffer_mgr_vbuffer_realloc(state.vertex_buffer, buffer_size);
			}
			catch(const graphics_exception& exception) {
				throw graphics_exception(
					fmt::format(strings::exceptions::g_models_failed_to_alloc_vbuffer, buffer_size).c_str()
				);
			}
		}

		void models__require_ibuffer_size(u32 buffer_size)
		{
			auto& state = g_models_state;
			try {
				if (state.index_buffer == no_index_buffer)
					state.index_buffer = buffer_mgr_ibuffer_create({
						strings::graphics::g_models_ibuffer_name,
						buffer_size,
						null,
						true
						});
				else
					buffer_mgr_ibuffer_realloc(state.index_buffer, buffer_size);
			}
			catch (const graphics_exception& exception) {
				throw graphics_exception(
					fmt::format(strings::exceptions::g_models_failed_to_alloc_ibuffer, buffer_size).c_str()
				);
			}
		}
		
		void models__prewarm_pipelines()
		{
			// prewarm shaders
			shader_create_desc shader_desc = {};
			shader_desc.name = strings::graphics::g_models_vshader_name;
			shader_desc.type = shader_type::vertex;
			shader_desc.source_code = strings::graphics::shaders::g_model_nouv_vertex;
			shader_desc.source_code_length = strlen(shader_desc.source_code);
			const auto& vertex = shader_mgr_create(shader_desc);

			shader_desc.name = strings::graphics::g_models_pshader_name;
			shader_desc.type = shader_type::pixel;
			shader_desc.source_code = strings::graphics::shaders::g_model_nouv_pixel;
			shader_desc.source_code_length = strlen(shader_desc.source_code);

			const auto& pixel = shader_mgr_create(shader_desc);

			graphics_pipeline_state_create create_desc = {};
			create_desc.name = strings::graphics::g_models_pipeline_name;
			create_desc.vertex_shader = vertex;
			create_desc.pixel_shader = pixel;
			create_desc.topology = primitive_topology::triangle_list;
			create_desc.num_render_targets = 1;
			create_desc.render_target_formats[0] = get_default_backbuffer_format();
			create_desc.depth_stencil_format = get_default_depthbuffer_format();
			create_desc.vertex_elements = (u32)vertex_elements::position | (u32)vertex_elements::color;

			for (u32 i = 0; i < (u8)primitive_topology::line_strip; ++i) {
				create_desc.topology = (primitive_topology)i;
				create_desc.wireframe = false;
				pipeline_state_mgr_create_graphics(create_desc);
				create_desc.wireframe = true;
				pipeline_state_mgr_create_graphics(create_desc);
			}
		}
	}
}