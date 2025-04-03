#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

namespace rengine {
	namespace graphics {
		struct shader_create_desc {
			c_str name;
			shader_type type;
			c_str source_code;
			u32 source_code_length;
			byte* bytecode;
			u32 bytecode_length;
		};

		R_EXPORT shader_t shader_mgr_create(const shader_create_desc& desc);
		R_EXPORT ptr shader_mgr_get_internal_handle(const shader_t& shader_id);
		R_EXPORT core::hash_t shader_mgr_hash_desc(const shader_create_desc& desc);
		R_EXPORT u32 shader_mgr_get_cache_count();
		R_EXPORT void shader_mgr_clear_cache();
	}
}