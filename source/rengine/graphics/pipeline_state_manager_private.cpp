#include "./pipeline_state_manager_private.h"
#include "./shader_manager_private.h"
#include "./graphics_private.h"
#include "./buffer_manager_private.h"

#include "../exceptions.h"

namespace rengine {
	namespace graphics {
		pipeline_state_mgr_state g_pipeline_state_mgr_state = {};

		void pipeline_state_mgr__deinit()
		{
			pipeline_state_mgr_clear_cache();
		}

		Diligent::IPipelineState* pipeline_state_mgr__create_graphics(const graphics_pipeline_state_create& create_info)
		{
			using namespace Diligent;
			const auto device = g_graphics_state.device;
			GraphicsPipelineStateCreateInfo ci = {};

			ci.PSODesc.Name = create_info.name;
			ci.pVS = shader_mgr__get_handle(create_info.vertex_shader);
			ci.pPS = shader_mgr__get_handle(create_info.pixel_shader);
			ci.GraphicsPipeline.NumRenderTargets = create_info.num_render_targets;
			ci.GraphicsPipeline.DSVFormat = (TEXTURE_FORMAT)create_info.depth_stencil_format;
			for (u8 i = 0; i < create_info.num_render_targets; ++i)
				ci.GraphicsPipeline.RTVFormats[i] = (TEXTURE_FORMAT)create_info.render_target_formats[i];
			ci.GraphicsPipeline.PrimitiveTopology = g_primitive_topology_tbl[(u8)create_info.topology];
			ci.GraphicsPipeline.RasterizerDesc.CullMode = g_cull_mode_tbl[(u8)create_info.cull];
			ci.GraphicsPipeline.RasterizerDesc.FillMode = create_info.wireframe ? FILL_MODE_WIREFRAME : FILL_MODE_SOLID;
			ci.GraphicsPipeline.RasterizerDesc.ScissorEnable = create_info.scissors;
			ci.GraphicsPipeline.DepthStencilDesc.DepthEnable = create_info.depth;

			ci.GraphicsPipeline.InputLayout.LayoutElements = pipeline_state_mgr__build_input_layout(create_info.vertex_elements,
				&ci.GraphicsPipeline.InputLayout.NumElements);

			ci.PSODesc.ResourceLayout.DefaultVariableType = SHADER_RESOURCE_VARIABLE_TYPE_MUTABLE;
			ci.PSODesc.ResourceLayout.Variables = pipeline_state_mgr__build_srv(&ci.PSODesc.ResourceLayout.NumVariables);

			IPipelineState* pipeline = null;
			device->CreateGraphicsPipelineState(ci, &pipeline);
			if (!pipeline)
				return null;

			pipeline->AddRef();
			pipeline_state_mgr__bind_cbuffers(pipeline);
			return pipeline;
		}

		Diligent::LayoutElement* pipeline_state_mgr__build_input_layout(u32 flags, u32* count)
		{
			using namespace Diligent;
			Diligent::LayoutElement* layout_elements = g_pipeline_state_mgr_state.tmp_layout_elements;

			*count = 0;
			if ((flags & (u32)vertex_elements::position) != 0) {
				auto& position_element = layout_elements[*count];
				position_element.InputIndex = 0;
				position_element.NumComponents = 3;
				position_element.ValueType = VT_FLOAT32;
				position_element.IsNormalized = false;

				(*count)++;
			}

			if ((flags & (u32)vertex_elements::normal) != 0) {
				LayoutElement& normal_element = layout_elements[*count];
				normal_element.InputIndex = 1;
				normal_element.NumComponents = 3;
				normal_element.ValueType = VT_FLOAT32;
				normal_element.RelativeOffset = sizeof(float) * 3;

				(*count)++;
			}

			if ((flags & (u32)vertex_elements::tangent) != 0) {
				LayoutElement tangent_element = layout_elements[*count];
				tangent_element.InputIndex = 2;
				tangent_element.NumComponents = 4;
				tangent_element.ValueType = VT_FLOAT32;
				tangent_element.RelativeOffset = sizeof(float) * 4;

				(*count)++;
			}

			if ((flags & (u32)vertex_elements::color) != 0) {
				LayoutElement& color_element = layout_elements[*count];
				color_element.InputIndex = 3;
				color_element.NumComponents = 1;
				color_element.ValueType = VT_UINT32;
				color_element.IsNormalized = false;

				(*count)++;
			}

			if ((flags & (u32)vertex_elements::texcoord) != 0) {
				LayoutElement& texcoord_element = layout_elements[*count];
				texcoord_element.InputIndex = 4;
				texcoord_element.NumComponents = 2;
				texcoord_element.ValueType = VT_FLOAT32;

				(*count)++;
			}
			
			if ((flags & (u32)vertex_elements::instancing) != 0) {
				(*count)++;
				throw not_implemented_exception();
			}

			return layout_elements;
		}

		Diligent::ShaderResourceVariableDesc* pipeline_state_mgr__build_srv(u32* count)
		{
			c_str cbuffer_keys[] = {
				strings::graphics::shaders::g_frame_buffer_key,
			};

			Diligent::SHADER_TYPE supported_shader_stages = Diligent::SHADER_TYPE_UNKNOWN;
			for (u8 i = 0; i < _countof(g_supported_shader_types); ++i)
				supported_shader_stages |= g_supported_shader_types[i];

			for (u8 i = 0; i < _countof(cbuffer_keys); ++i) {
				auto& srv = g_pipeline_state_mgr_state.tmp_srv[i];
				srv.Name = cbuffer_keys[i];
				srv.Flags = Diligent::SHADER_VARIABLE_FLAG_NONE;
				srv.ShaderStages = supported_shader_stages;
				srv.Type = Diligent::SHADER_RESOURCE_VARIABLE_TYPE_STATIC;
			}

			*count = _countof(cbuffer_keys);
			return g_pipeline_state_mgr_state.tmp_srv;
		}

		void pipeline_state_mgr__get_internal_handle(const pipeline_state_t& id, Diligent::IPipelineState** output)
		{
			const auto& state = g_pipeline_state_mgr_state;
			if (!output)
				return;

			const auto& it = state.pipelines.find_as(id);
			if (it == state.pipelines.end())
				return;
			*output = it->second;
		}
		
		void pipeline_state_mgr__bind_cbuffers(Diligent::IPipelineState* pipeline)
		{
			c_str cbuffer_keys[] = {
				strings::graphics::shaders::g_frame_buffer_key,
			};
			constant_buffer_t cbuffer_values[] = {
				g_graphics_state.buffers.frame
			};

			for (u8 i = 0; i < _countof(cbuffer_values); ++i) {
				const auto& key = cbuffer_keys[i];
				const auto& value = cbuffer_values[i];
				Diligent::IBuffer* cbuffer = null;
				buffer_mgr__get_handle(buffer_type::constant_buffer, value, &cbuffer);

				for (u8 j = 0; j < _countof(g_supported_shader_types); ++j) {
					auto& shader_type = g_supported_shader_types[j];
					auto var = pipeline->GetStaticVariableByName(shader_type, key);
					if (var)
						var->Set(cbuffer);
				}
			}
		}
	}
}