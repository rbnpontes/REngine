#pragma once
#include <rengine/types.h>


namespace rengine {
	namespace graphics {
		enum class srb_mgr_resource_type {
			unknow = 0,
			tex2d,
			tex3d,
			texcube,
			texarray,
			rt
		};

		struct srb_mgr_resource_desc {
			c_str name;
			entity id;
			srb_mgr_resource_type type;
		};

		struct srb_mgr_create_desc {
			pipeline_state_t pipeline{ no_pipeline_state };
			srb_mgr_resource_desc* resources{ null };
			u8 num_resources{ 0 };
		};

		struct srb_mgr_update_desc {
			srb_t id;
			srb_mgr_resource_desc* resources{ null };
			u8 num_resources{ 0 };
		};

		srb_t srb_mgr_create(const srb_mgr_create_desc& desc);
		void srb_mgr_update(const srb_mgr_update_desc& desc);
		void srb_mgr_clear_cache();
		void srb_mgr_get_handle(const srb_t id, ptr* srb_handle_out);
		u32 srb_mgr_get_count();
	}
}