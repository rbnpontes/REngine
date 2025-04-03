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
			Diligent::CULL_MODE_FRONT
		};

		extern hash_map<pipeline_state_t, Diligent::IPipelineState*> g_cached_pipelines;
		extern u32 g_cached_pipelines_count;

		Diligent::IPipelineState* pipeline_state_mgr__create_graphics(const graphics_pipeline_state_create& create_info);
		void pipeline_state_mgr__build_input_layout(u32 flags, vector<Diligent::LayoutElement>& elements);
	}
}