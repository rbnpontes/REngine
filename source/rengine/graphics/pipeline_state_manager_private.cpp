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
			ci.GraphicsPipeline.BlendDesc.AlphaToCoverageEnable = create_info.alpha_to_coverage;
			ci.GraphicsPipeline.BlendDesc.IndependentBlendEnable = false;
			ci.GraphicsPipeline.SmplDesc.Count = create_info.msaa_level;

			pipeline_state_mgr__fill_rasterizer(&ci, create_info);
			pipeline_state_mgr__fill_blend_desc(&ci, create_info);
			pipeline_state_mgr__fill_depth_stencil(&ci, create_info);

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

			const auto program = shader_mgr__get_program(program_id);
			const auto shader_ids = reinterpret_cast<const shader_t*>(&program->desc);
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

		void pipeline_state_mgr__fill_rasterizer(Diligent::GraphicsPipelineStateCreateInfo* ci, const graphics_pipeline_state_create& create_info)
		{
			using namespace Diligent;
			auto curr_backend = g_graphics_state.backend;
			auto is_opengl = curr_backend == backend::opengl;
			auto is_vulkan = curr_backend == backend::vulkan;

			auto depth_bits = 24;
			if (TEX_FORMAT_D16_UNORM == create_info.depth_stencil_format)
				depth_bits = 16;

			// opengl doesn't supports depth bias
			const auto scaled_depth_bias = is_opengl 
				? 0 
				: (create_info.constant_depth_bias * (1 << depth_bits));


			auto& rasterizer_desc = ci->GraphicsPipeline.RasterizerDesc;
			rasterizer_desc.CullMode = g_cull_mode_tbl[(u8)create_info.cull];
			rasterizer_desc.FillMode = create_info.wireframe ? FILL_MODE_WIREFRAME : FILL_MODE_SOLID;
			rasterizer_desc.FrontCounterClockwise = false;
			rasterizer_desc.DepthBias = scaled_depth_bias;
			if (is_opengl || is_vulkan)
				rasterizer_desc.DepthBiasClamp = 0;
			else
				rasterizer_desc.DepthBiasClamp = MAX_FLOAT_VALUE;
			rasterizer_desc.SlopeScaledDepthBias = create_info.slope_scaled_depth_bias;
			rasterizer_desc.DepthClipEnable = true;
			rasterizer_desc.ScissorEnable = create_info.scissors;
			rasterizer_desc.AntialiasedLineEnable = create_info.msaa_level != 1;
			rasterizer_desc.DepthBias = create_info.constant_depth_bias;
		}

		void pipeline_state_mgr__fill_blend_desc(Diligent::GraphicsPipelineStateCreateInfo* ci, const graphics_pipeline_state_create& create_info)
		{
			if (create_info.num_render_targets == 0)
				return;

			auto& blend_desc = ci->GraphicsPipeline.BlendDesc.RenderTargets[0];
			blend_desc.BlendEnable = create_info.blend_mode == blend_mode::replace;
			blend_desc.SrcBlend = g_source_blends_tbl[(u8)create_info.blend_mode];
			blend_desc.DestBlend = g_dest_blends_tbl[(u8)create_info.blend_mode];
			blend_desc.BlendOp = g_blend_operation_tbl[(u8)create_info.blend_mode];
			blend_desc.SrcBlendAlpha = g_source_blends_tbl[(u8)create_info.blend_mode];
			blend_desc.DestBlendAlpha = g_dest_blends_tbl[(u8)create_info.blend_mode];
			blend_desc.BlendOpAlpha = g_blend_operation_tbl[(u8)create_info.blend_mode];
			blend_desc.RenderTargetWriteMask = create_info.color_write
				? Diligent::COLOR_MASK_ALL
				: Diligent::COLOR_MASK_NONE;
		}

		void pipeline_state_mgr__fill_depth_stencil(Diligent::GraphicsPipelineStateCreateInfo* ci, const graphics_pipeline_state_create& create_info)
		{
			auto& stencil_desc = create_info.depth_desc;
			auto& depth_stencil_desc = ci->GraphicsPipeline.DepthStencilDesc;
			depth_stencil_desc.DepthEnable = stencil_desc.depth_enabled;
			depth_stencil_desc.DepthWriteEnable = stencil_desc.depth_write;
			depth_stencil_desc.DepthFunc = g_comparison_function_tbl[(u8)stencil_desc.depth_cmp_func];
			depth_stencil_desc.StencilEnable = stencil_desc.stencil_test;
			depth_stencil_desc.StencilReadMask = stencil_desc.cmp_mask;
			depth_stencil_desc.StencilWriteMask = stencil_desc.write_mask;
			depth_stencil_desc.FrontFace.StencilFailOp = g_stencil_op_tbl[(u8)stencil_desc.on_stencil];
			depth_stencil_desc.FrontFace.StencilDepthFailOp = g_stencil_op_tbl[(u8)stencil_desc.on_depth_fail];
			depth_stencil_desc.FrontFace.StencilPassOp = g_stencil_op_tbl[(u8)stencil_desc.on_passed];
			depth_stencil_desc.FrontFace.StencilFunc = g_comparison_function_tbl[(u8)stencil_desc.stencil_cmp_func];
			depth_stencil_desc.BackFace.StencilFailOp = g_stencil_op_tbl[(u8)stencil_desc.on_stencil];
			depth_stencil_desc.BackFace.StencilDepthFailOp = g_stencil_op_tbl[(u8)stencil_desc.on_depth_fail];
			depth_stencil_desc.BackFace.StencilPassOp = g_stencil_op_tbl[(u8)stencil_desc.on_passed];
			depth_stencil_desc.BackFace.StencilFunc = g_comparison_function_tbl[(u8)stencil_desc.stencil_cmp_func];
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
				desc.Desc.MinFilter =
					desc.Desc.MipFilter = 
					desc.Desc.MagFilter = g_filter_type_tbl[(u32)sampler.desc.filter];
				desc.Desc.AddressU = 
					desc.Desc.AddressV =
					desc.Desc.AddressW = g_texture_address_mode_tbl[(u32)sampler.desc.address];
				desc.Desc.ComparisonFunc = g_comparison_function_tbl[(u32)sampler.desc.comparison];
				desc.Desc.MinLOD = sampler.desc.min_lod;
				desc.Desc.MaxLOD = sampler.desc.max_lod;
				desc.Desc.MipLODBias = sampler.desc.lod_bias;
				desc.Desc.MaxAnisotropy = sampler.desc.max_anisotropy;
				desc.Desc.Name = sampler.name;
				desc.Desc.Flags = SAMPLER_FLAG_NONE;
				desc.Desc.UnnormalizedCoords = false;
				memset(desc.Desc.BorderColor, 0x0, sizeof(float) * 4);
			}
			return immutable_samplers;
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