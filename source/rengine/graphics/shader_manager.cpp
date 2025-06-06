#include "./shader_manager.h"
#include "./shader_manager_private.h"

#include "../core/allocator.h"
#include "../core/hash.h"
#include "../exceptions.h"
#include "../strings.h"

namespace rengine {
	namespace graphics {
		shader_t shader_mgr_create(const shader_create_desc& desc) {
			auto& state = g_shader_mgr_state;
			const auto id = shader_mgr_hash_desc(desc);
			const auto it = state.shaders.find_as(id);
			if (it != state.shaders.end())
				return id;

			const auto shader = shader_mgr__create_shader(desc);
			if (!shader)
				throw graphics_exception(strings::exceptions::g_shader_mgr_fail_to_create_shader);

			const auto resource_count = shader->GetResourceCount();
			shader_entry entry{ 
				shader, 
				resource_count,
				core::alloc_array_alloc<shader_resource>(resource_count) 
			};
			shader_mgr__collect_resources(shader, entry.resources);

			++state.shaders_count;
			state.shaders[id] = entry;
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

		void shader_mgr_get_resources(const shader_t& shader_id, u32* num_resources, shader_resource* output_resources)
		{
			const auto& state = g_shader_mgr_state;
			if (!num_resources)
				return;

			const auto& it = state.shaders.find_as(shader_id);
			if (it == state.shaders.end())
				return;

			const auto& entry = it->second;
			*num_resources = entry.num_resources;
			
			if (!output_resources)
				return;
		
			memcpy(output_resources, entry.resources, sizeof(shader_resource) * entry.num_resources);
		}

		u32 shader_mgr_get_cache_count()
		{
			return g_shader_mgr_state.shaders_count;
		}

		void shader_mgr_clear_cache() {
			auto& state = g_shader_mgr_state;
			for (const auto& it : state.shaders)
				shader_mgr__free(it.second);

			state.shaders.clear();
			state.shaders_count = 0;
		}
	}
}