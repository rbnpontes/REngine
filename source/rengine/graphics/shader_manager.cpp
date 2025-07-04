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
				core::alloc_array_alloc<shader_resource>(resource_count),
				desc.vertex_elements
			};
			shader_mgr__collect_resources(shader, entry.resources, (u32)g_shader_type_flags_tbl[(u8)desc.type]);

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
			return shader_mgr__hash_desc(desc);
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

		u32 shader_mgr_get_vertex_elements(const shader_t& shader_id)
		{
			auto& state = g_shader_mgr_state;
			if (shader_id == no_shader)
				return (u32)vertex_elements::none;
			const auto it = state.shaders.find_as(shader_id);
			if (it == state.shaders.end())
				return (u32)vertex_elements::none;

			return it->second.vertex_elements;
		}

		shader_program_t shader_mgr_create_program(const shader_program_create_desc& desc)
		{
			return shader_mgr__create_program(desc);
		}

		void shader_mgr_free_program(const shader_program_t& program_id)
		{
			if (program_id == no_shader_program)
				return;

			auto& state = g_shader_mgr_state;
			const auto it = state.programs.find_as(program_id);
			if (it == state.programs.end())
				return;

			state.programs.erase(it);
			--state.programs_count;
		}

		void shader_mgr_clear_program_cache()
		{
			auto& state = g_shader_mgr_state;
			state.programs.clear();
			state.programs_count = 0;
		}

		u32 shader_mgr_get_program_cache_count()
		{
			return g_shader_mgr_state.programs_count;
		}
	}
}