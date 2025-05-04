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

		struct pipeline_state_mgr_state {
			hash_map<pipeline_state_t, Diligent::IPipelineState*> pipelines;
			u32 pipeline_count;

			Diligent::LayoutElement tmp_layout_elements[6];
			Diligent::ShaderResourceVariableDesc tmp_srv[4];
		};

		extern pipeline_state_mgr_state g_pipeline_state_mgr_state;

		void pipeline_state_mgr__deinit();

		Diligent::IPipelineState* pipeline_state_mgr__create_graphics(const graphics_pipeline_state_create& create_info);
		Diligent::LayoutElement* pipeline_state_mgr__build_input_layout(u32 flags, u32* count);
		Diligent::ShaderResourceVariableDesc* pipeline_state_mgr__build_srv(u32* count);
		
		void pipeline_state_mgr__get_internal_handle(const pipeline_state_t& id, Diligent::IPipelineState** output);

		void pipeline_state_mgr__bind_cbuffers(Diligent::IPipelineState* pipeline);
	}
}