#include "./srb_manager.h"
#include "./srb_manager_private.h"
#include "./pipeline_state_manager_private.h"

#include "../exceptions.h"

#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		srb_t srb_mgr_create(const srb_mgr_create_desc& desc)
		{
			auto& state = g_srb_mgr_state;
			const auto& hash = srb_mgr__build_hash(desc.pipeline, desc.resources, desc.num_resources);
			const auto& it = g_srb_mgr_state.srb_tbl.find_as(hash);

			if (it != g_srb_mgr_state.srb_tbl.end())
				return it->second;

			Diligent::IPipelineState* pipeline = null;
			pipeline_state_mgr__get_internal_handle(desc.pipeline, &pipeline);

			if (!pipeline)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_srb_invalid_pipeline, desc.pipeline).c_str()
				);

			auto srb = srb_mgr__create(pipeline, desc);
			const auto result = g_srb_mgr_state.entries.size();
			state.entries.push_back({
				hash,
				srb,
				desc.pipeline
			});
			state.srb_tbl[hash] = result;

			return result;
		}

		void srb_mgr_update(const srb_mgr_update_desc& desc)
		{
			auto& state = g_srb_mgr_state;
			if (!srb_mgr__assert_id(desc.id))
				return;

			const auto& entry = state.entries[desc.id];
			const auto hash = srb_mgr__build_hash(entry.pipeline, desc.resources, desc.num_resources);

			// if hash is same, there's no reason to update SRB
			if (hash == entry.id)
				return;

			state.srb_tbl.erase(state.srb_tbl.find_as(desc.id));

			srb_mgr__set_resources(entry.handle, desc.resources, desc.num_resources);

			state.srb_tbl[hash] = desc.id;
		}

		void srb_mgr_clear_cache()
		{
			for (auto& entry : g_srb_mgr_state.entries)
				entry.handle->Release();
			g_srb_mgr_state.entries.clear();
			g_srb_mgr_state.srb_tbl.clear();
		}

		void srb_mgr_get_handle(const srb_t id, ptr* srb_handle_out)
		{
			srb_mgr__get_handle(id, (Diligent::IShaderResourceBinding**)srb_handle_out);
		}

		u32 srb_mgr_get_count()
		{
			return g_srb_mgr_state.entries.size();
		}
	}
}