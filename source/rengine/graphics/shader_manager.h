#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

namespace rengine {
	namespace graphics {

		struct shader_macro {
			c_str name;
			c_str definition;
		};

		struct shader_create_desc {
			c_str name{ null };
			shader_type type{ shader_type::vertex };
			c_str source_code{ null };
			u32 source_code_length{ 0 };
			byte* bytecode{ null };
			u32 bytecode_length{ 0 };

			shader_macro* macros{ null };
			u32 num_macros{ 0 };
		};

		R_EXPORT shader_t shader_mgr_create(const shader_create_desc& desc);
		R_EXPORT ptr shader_mgr_get_internal_handle(const shader_t& shader_id);
		R_EXPORT core::hash_t shader_mgr_hash_desc(const shader_create_desc& desc);
		R_EXPORT u32 shader_mgr_get_cache_count();
		R_EXPORT void shader_mgr_clear_cache();
	}
}