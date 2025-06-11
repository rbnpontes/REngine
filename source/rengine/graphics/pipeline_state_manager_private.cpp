#include "./pipeline_state_manager_private.h"
#include "./shader_manager_private.h"
#include "./graphics_private.h"
#include "./buffer_manager_private.h"
#include "./render_target_manager.h"

#include "../core/allocator.h"

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
			u32 vertex_elements = (u32)vertex_elements::none;
			GraphicsPipelineStateCreateInfo ci = {};

			ci.PSODesc.Name = create_info.name;
			pipeline_state_mgr__fill_shaders(&ci, create_info.shader_program, &vertex_elements);
			ci.GraphicsPipeline.NumRenderTargets = create_info.num_render_targets;
			ci.GraphicsPipeline.DSVFormat = (TEXTURE_FORMAT)create_info.depth_stencil_format;
			for (u8 i = 0; i < create_info.num_render_targets; ++i)
				ci.GraphicsPipeline.RTVFormats[i] = (TEXTURE_FORMAT)create_info.render_target_formats[i];
			ci.GraphicsPipeline.PrimitiveTopology = g_primitive_topology_tbl[(u8)create_info.topology];
			ci.GraphicsPipeline.RasterizerDesc.CullMode = g_cull_mode_tbl[(u8)create_info.cull];
			ci.GraphicsPipeline.RasterizerDesc.FillMode = create_info.wireframe ? FILL_MODE_WIREFRAME : FILL_MODE_SOLID;
			ci.GraphicsPipeline.RasterizerDesc.ScissorEnable = create_info.scissors;
			ci.GraphicsPipeline.RasterizerDesc.AntialiasedLineEnable = create_info.msaa_level != 1;
			ci.GraphicsPipeline.SmplDesc.Count = create_info.msaa_level;
			ci.GraphicsPipeline.DepthStencilDesc.DepthEnable = create_info.depth;

			ci.GraphicsPipeline.InputLayout.LayoutElements = pipeline_state_mgr__build_input_layout(
				vertex_elements,
				&ci.GraphicsPipeline.InputLayout.NumElements);

			ci.PSODesc.ResourceLayout.DefaultVariableType = SHADER_RESOURCE_VARIABLE_TYPE_MUTABLE;
			ci.PSODesc.ResourceLayout.Variables = pipeline_state_mgr__build_srv(&ci.PSODesc.ResourceLayout.NumVariables);
			ci.PSODesc.ResourceLayout.NumImmutableSamplers = create_info.num_immutable_samplers;
			ci.PSODesc.ResourceLayout.ImmutableSamplers = pipeline_state_mgr__build_immutable_samplers(create_info);

			IPipelineState* pipeline = null;
			device->CreateGraphicsPipelineState(ci, &pipeline);

			// free scratch memory
			core::alloc_scratch_pop(
				sizeof(Diligent::LayoutElement) * VERTEX_ELEMENT_COUNT +
				sizeof(Diligent::ImmutableSamplerDesc) * GRAPHICS_MAX_BOUND_TEXTURES +
				sizeof(Diligent::ShaderResourceVariableDesc) * GRAPHICS_MAX_BOUND_CBUFFERS
			);

			if (!pipeline)
				return null;

			pipeline_state_mgr__bind_cbuffers(pipeline);
			return pipeline;
		}

		void pipeline_state_mgr__fill_shaders(Diligent::GraphicsPipelineStateCreateInfo* ci, shader_program_t program_id, u32* vertex_elements)
		{
			using namespace Diligent;
			if (program_id == no_shader_program)
				return;

			shader_program program;
			shader_mgr__get_program(program_id, &program);
			shader_t* shader_ids = reinterpret_cast<shader_t*>(&program.desc);
			IShader** shader_outputs[(u8)shader_type::max] = {
				&ci->pVS,
				&ci->pPS
			};

			*vertex_elements = shader_mgr_get_vertex_elements(shader_ids[(u8)shader_type::vertex]);

			for (u8 i = 0; i < (u8)shader_type::max; ++i)
				*shader_outputs[i] = shader_mgr__get_handle(shader_ids[i]);

			if (ci->pVS == null || ci->pPS == null)
				throw graphics_exception(strings::exceptions::g_pipeline_state_mgr_required_vs_ps_shaders);
		}

		Diligent::LayoutElement* pipeline_state_mgr__build_input_layout(u32 flags, u32* count)
		{
			using namespace Diligent;
			auto layout_elements = (Diligent::LayoutElement*)core::alloc_scratch(
				sizeof(Diligent::LayoutElement) * VERTEX_ELEMENT_COUNT
			);

			*count = 0;
			if ((flags & (u32)vertex_elements::position) != 0) {
				auto& position_element = layout_elements[*count];
				position_element = {};
				position_element.InputIndex = VERTEX_ELEMENT_POSITION_IDX;
				position_element.NumComponents = 3;
				position_element.ValueType = VT_FLOAT32;
				position_element.IsNormalized = false;

				(*count)++;
			}

			if ((flags & (u32)vertex_elements::normal) != 0) {
				LayoutElement& normal_element = layout_elements[*count];
				normal_element = {};
				normal_element.InputIndex = VERTEX_ELEMENT_NORMAL_IDX;
				normal_element.NumComponents = 3;
				normal_element.ValueType = VT_FLOAT32;
				normal_element.RelativeOffset = sizeof(float) * 3;

				(*count)++;
			}

			if ((flags & (u32)vertex_elements::tangent) != 0) {
				LayoutElement tangent_element = layout_elements[*count];
				tangent_element = {};
				tangent_element.InputIndex = VERTEX_ELEMENT_TANGENT_IDX;
				tangent_element.NumComponents = 4;
				tangent_element.ValueType = VT_FLOAT32;
				tangent_element.RelativeOffset = sizeof(float) * 4;

				(*count)++;
			}

			if ((flags & (u32)vertex_elements::color) != 0) {
				LayoutElement& color_element = layout_elements[*count];
				color_element = {};
				color_element.InputIndex = VERTEX_ELEMENT_COLOR_IDX;
				color_element.NumComponents = 1;
				color_element.ValueType = VT_UINT32;
				color_element.IsNormalized = false;

				(*count)++;
			}

			if ((flags & (u32)vertex_elements::colorf) != 0) {
				LayoutElement& colorf_element = layout_elements[*count];
				colorf_element = {};
				colorf_element.InputIndex = VERTEX_ELEMENT_COLORF_IDX;
				colorf_element.NumComponents = 4;
				colorf_element.ValueType = VT_FLOAT32;
				colorf_element.RelativeOffset = sizeof(float) * 4;
				colorf_element.IsNormalized = false;
				(*count)++;
			}

			if ((flags & (u32)vertex_elements::uv) != 0) {
				LayoutElement& texcoord_element = layout_elements[*count];
				texcoord_element = {};
				texcoord_element.InputIndex = VERTEX_ELEMENT_UV_IDX;
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

		Diligent::ImmutableSamplerDesc* pipeline_state_mgr__build_immutable_samplers(const graphics_pipeline_state_create& create_info)
		{
			using namespace Diligent;
			if (create_info.num_immutable_samplers == 0)
				return null;

			auto immutable_samplers = (Diligent::ImmutableSamplerDesc*)core::alloc_scratch(
				sizeof(Diligent::ImmutableSamplerDesc) * create_info.num_immutable_samplers
			);
			for (auto i = 0; i < create_info.num_immutable_samplers; ++i) {
				const auto& sampler = create_info.immutable_samplers[i];
				auto& desc = immutable_samplers[i];
				desc.SamplerOrTextureName = sampler.name;
				desc.ShaderStages = (Diligent::SHADER_TYPE)sampler.shader_type_flags;
				desc.Desc.MinFilter = g_filter_type_tbl[(u32)sampler.desc.filter];
				desc.Desc.MipFilter = g_filter_type_tbl[(u32)sampler.desc.filter];
				desc.Desc.MagFilter = g_filter_type_tbl[(u32)sampler.desc.filter];
				desc.Desc.AddressU = g_texture_address_mode_tbl[(u32)sampler.desc.address];
				desc.Desc.AddressV = g_texture_address_mode_tbl[(u32)sampler.desc.address];
				desc.Desc.AddressW = g_texture_address_mode_tbl[(u32)sampler.desc.address];
				desc.Desc.ComparisonFunc = g_comparison_function_tbl[(u32)sampler.desc.comparison];
				desc.Desc.MinLOD = sampler.desc.min_lod;
				desc.Desc.MaxLOD = sampler.desc.max_lod;
				desc.Desc.MipLODBias = sampler.desc.lod_bias;
				desc.Desc.Name = sampler.name;
			}
			return null;
		}

		Diligent::ShaderResourceVariableDesc* pipeline_state_mgr__build_srv(u32* count)
		{
			auto srv_list = (Diligent::ShaderResourceVariableDesc*)core::alloc_scratch(
				sizeof(Diligent::ShaderResourceVariableDesc) * GRAPHICS_MAX_BOUND_CBUFFERS
			);
			c_str cbuffer_keys[] = {
				strings::graphics::shaders::g_frame_buffer_key,
			};

			Diligent::SHADER_TYPE supported_shader_stages = Diligent::SHADER_TYPE_UNKNOWN;
			for (u8 i = 0; i < _countof(g_supported_shader_types); ++i)
				supported_shader_stages |= g_supported_shader_types[i];

			for (u8 i = 0; i < _countof(cbuffer_keys); ++i) {
				auto& srv = srv_list[i];
				srv.Name = cbuffer_keys[i];
				srv.ShaderStages = supported_shader_stages;
				srv.Flags = Diligent::SHADER_VARIABLE_FLAG_NONE;
				srv.Type = Diligent::SHADER_RESOURCE_VARIABLE_TYPE_STATIC;
			}

			*count = _countof(cbuffer_keys);
			return srv_list;
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