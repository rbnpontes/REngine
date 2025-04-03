#include "./shader_manager.h"
#include "./shader_manager_private.h"

#include "../core/hash.h"
#include "../exceptions.h"
#include "../strings.h"

namespace rengine {
	namespace graphics {
		shader_t shader_mgr_create(const shader_create_desc& desc) {
			const auto id = shader_mgr_hash_desc(desc);
			const auto it = g_shader_tbl.find_as(id);
			if (it != g_shader_tbl.end())
				return id;

			const auto shader = shader_mgr__create_shader(desc);
			if (!shader)
				throw graphics_exception(strings::exceptions::g_shader_mgr_fail_to_create_shader);

			++g_shader_count;
			g_shader_tbl[id] = shader;
			return id;
		}

		ptr shader_mgr_get_internal_handle(const shader_t& shader_id)
		{
			return shader_mgr__get_handle(shader_id);
		}

		core::hash_t shader_mgr_hash_desc(const shader_create_desc& desc)
		{
			core::hash_t result = core::hash(desc.name);
			result = core::hash_combine(result, (core::hash_t)desc.type);
			result = core::hash_combine(result, core::hash(desc.name));
			result = core::hash_combine(result, desc.source_code_length);
			result = core::hash_combine(result, core::hash(desc.bytecode, desc.bytecode_length));
			result = core::hash_combine(result, desc.bytecode_length);
			return result;
		}

		u32 shader_mgr_get_cache_count()
		{
			return g_shader_count;
		}

		void shader_mgr_clear_cache() {
			for (const auto& it : g_shader_tbl)
				it.second->Release();
			g_shader_tbl.clear();
			g_shader_count = 0;
		}
	}
}