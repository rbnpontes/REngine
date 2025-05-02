#include "./pipeline_state_manager_private.h"
#include "./shader_manager_private.h"
#include "./graphics_private.h"

#include "../exceptions.h"

namespace rengine {
	namespace graphics {
		hash_map<pipeline_state_t, Diligent::IPipelineState*> g_cached_pipelines = {};
		u32 g_cached_pipelines_count = 0;

		Diligent::IPipelineState* pipeline_state_mgr__create_graphics(const graphics_pipeline_state_create& create_info)
		{
			using namespace Diligent;
			const auto device = g_graphics_state.device;
			vector<Diligent::LayoutElement> layout_elements;
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

			pipeline_state_mgr__build_input_layout(create_info.vertex_elements, layout_elements);
			ci.GraphicsPipeline.InputLayout.NumElements = layout_elements.size();
			ci.GraphicsPipeline.InputLayout.LayoutElements = layout_elements.data();

			IPipelineState* pipeline = null;
			device->CreateGraphicsPipelineState(ci, &pipeline);
			if (pipeline)
				pipeline->AddRef();
			return pipeline;
		}

		void pipeline_state_mgr__build_input_layout(u32 flags, vector<Diligent::LayoutElement>& elements)
		{
			using namespace Diligent;
			if ((flags & (u32)vertex_elements::position) != 0) {
				LayoutElement position_element = {};
				position_element.InputIndex = 0;
				position_element.NumComponents = 3;
				position_element.ValueType = VT_FLOAT32;
				//position_element.RelativeOffset = sizeof(float) * 3;
				position_element.IsNormalized = false;

				elements.push_back(position_element);
			}

			if ((flags & (u32)vertex_elements::normal) != 0) {
				LayoutElement normal_element = {};
				normal_element.InputIndex = 1;
				normal_element.NumComponents = 3;
				normal_element.ValueType = VT_FLOAT32;
				normal_element.RelativeOffset = sizeof(float) * 3;

				elements.push_back(normal_element);
			}

			if ((flags & (u32)vertex_elements::tangent) != 0) {
				LayoutElement tangent_element = {};
				tangent_element.InputIndex = 2;
				tangent_element.NumComponents = 4;
				tangent_element.ValueType = VT_FLOAT32;
				tangent_element.RelativeOffset = sizeof(float) * 4;

				elements.push_back(tangent_element);
			}

			if ((flags & (u32)vertex_elements::color) != 0) {
				LayoutElement color_element = {};
				color_element.InputIndex = 3;
				color_element.NumComponents = 1;
				color_element.ValueType = VT_UINT32;
				color_element.IsNormalized = false;
				//color_element.RelativeOffset = sizeof(u32);

				elements.push_back(color_element);
			}

			if ((flags & (u32)vertex_elements::texcoord) != 0) {
				LayoutElement texcoord_element = {};
				texcoord_element.InputIndex = 4;
				texcoord_element.NumComponents = 2;
				texcoord_element.ValueType = VT_FLOAT32;
				//texcoord_element.RelativeOffset = sizeof(float) * 2;

				elements.push_back(texcoord_element);
			}

			if ((flags & (u32)vertex_elements::instancing) != 0)
				throw not_implemented_exception();
		}

		void pipeline_state_mgr__get_internal_handle(const pipeline_state_t& id, Diligent::IPipelineState** output)
		{
			if (!output)
				return;

			const auto& it = g_cached_pipelines.find_as(id);
			if (it == g_cached_pipelines.end())
				return;
			*output = it->second;
		}
	}
}