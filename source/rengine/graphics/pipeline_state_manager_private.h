#pragma once
#include "../base_private.h"
#include "./pipeline_state_manager.h"

#include <GraphicsTypes.h>
#include <PipelineState.h>

namespace rengine {
	namespace graphics {
		static constexpr Diligent::PRIMITIVE_TOPOLOGY g_primitive_topology_tbl[] = {
			Diligent::PRIMITIVE_TOPOLOGY_TRIANGLE_LIST,
			Diligent::PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP,
			Diligent::PRIMITIVE_TOPOLOGY_POINT_LIST,
			Diligent::PRIMITIVE_TOPOLOGY_LINE_LIST,
			Diligent::PRIMITIVE_TOPOLOGY_LINE_STRIP
		};
		static constexpr Diligent::CULL_MODE g_cull_mode_tbl[] = {
			Diligent::CULL_MODE_NONE,
			Diligent::CULL_MODE_BACK,
			Diligent::CULL_MODE_FRONT,
		};
		static constexpr Diligent::SHADER_TYPE g_supported_shader_types[] = {
			Diligent::SHADER_TYPE_VERTEX,
			Diligent::SHADER_TYPE_PIXEL
		};
		static constexpr Diligent::FILTER_TYPE g_filter_type_tbl[] = {
			Diligent::FILTER_TYPE_UNKNOWN,
			Diligent::FILTER_TYPE_POINT,
			Diligent::FILTER_TYPE_LINEAR,
			Diligent::FILTER_TYPE_ANISOTROPIC,
			Diligent::FILTER_TYPE_COMPARISON_POINT,
			Diligent::FILTER_TYPE_COMPARISON_LINEAR,
			Diligent::FILTER_TYPE_COMPARISON_ANISOTROPIC,
			Diligent::FILTER_TYPE_MINIMUM_POINT,
			Diligent::FILTER_TYPE_MINIMUM_LINEAR,
			Diligent::FILTER_TYPE_MINIMUM_ANISOTROPIC,
			Diligent::FILTER_TYPE_MAXIMUM_POINT,
			Diligent::FILTER_TYPE_MAXIMUM_LINEAR,
			Diligent::FILTER_TYPE_MAXIMUM_ANISOTROPIC
		};
		static constexpr Diligent::TEXTURE_ADDRESS_MODE g_texture_address_mode_tbl[] = {
			Diligent::TEXTURE_ADDRESS_UNKNOWN,
			Diligent::TEXTURE_ADDRESS_WRAP,
			Diligent::TEXTURE_ADDRESS_MIRROR,
			Diligent::TEXTURE_ADDRESS_CLAMP,
			Diligent::TEXTURE_ADDRESS_BORDER,
			Diligent::TEXTURE_ADDRESS_MIRROR_ONCE
		};
		static constexpr Diligent::COMPARISON_FUNCTION g_comparison_function_tbl[] = {
			Diligent::COMPARISON_FUNC_UNKNOWN,
			Diligent::COMPARISON_FUNC_NEVER,
			Diligent::COMPARISON_FUNC_LESS,
			Diligent::COMPARISON_FUNC_EQUAL,
			Diligent::COMPARISON_FUNC_LESS_EQUAL,
			Diligent::COMPARISON_FUNC_GREATER,
			Diligent::COMPARISON_FUNC_NOT_EQUAL,
			Diligent::COMPARISON_FUNC_GREATER_EQUAL,
			Diligent::COMPARISON_FUNC_ALWAYS
		};

		struct pipeline_state_mgr_state {
			hash_map<pipeline_state_t, Diligent::IPipelineState*> pipelines;
			u32 pipeline_count;
		};

		extern pipeline_state_mgr_state g_pipeline_state_mgr_state;

		void pipeline_state_mgr__deinit();

		Diligent::IPipelineState* pipeline_state_mgr__create_graphics(const graphics_pipeline_state_create& create_info);
		void pipeline_state_mgr__fill_shaders(Diligent::GraphicsPipelineStateCreateInfo* ci, shader_program_t program_id, u32* vertex_elements);
		Diligent::LayoutElement* pipeline_state_mgr__build_input_layout(u32 flags, u32* count);
		Diligent::ImmutableSamplerDesc* pipeline_state_mgr__build_immutable_samplers(const graphics_pipeline_state_create& create_info);
		
		Diligent::ShaderResourceVariableDesc* pipeline_state_mgr__build_srv(u32* count);
		
		void pipeline_state_mgr__get_internal_handle(const pipeline_state_t& id, Diligent::IPipelineState** output);

		void pipeline_state_mgr__bind_cbuffers(Diligent::IPipelineState* pipeline);
	}
}