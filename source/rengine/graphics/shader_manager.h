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

			// specify vertex elements as hint for shader
			// the shader manager will insert required macros
			// to enable or disable some vertex elements
			u32 vertex_elements{ (u32)vertex_elements::none };
		};

		struct shader_resource {
			core::hash_t id{};
			resource_type type{ resource_type::unknow };
			c_str name{};
			u32 shader_flags{ (u32)shader_type_flags::none };
		};

		struct shader_program_desc {
			shader_t vertex_shader{ no_shader };
			shader_t pixel_shader{ no_shader };
		};
		struct shader_program_create_desc {
			shader_program_desc desc{};
		};

		R_EXPORT shader_t shader_mgr_create(const shader_create_desc& desc);
		R_EXPORT ptr shader_mgr_get_internal_handle(const shader_t& shader_id);
		R_EXPORT core::hash_t shader_mgr_hash_desc(const shader_create_desc& desc);
		R_EXPORT void shader_mgr_get_resources(const shader_t& shader_id, u32* num_resources, shader_resource* output_resources);
		R_EXPORT u32 shader_mgr_get_cache_count();
		R_EXPORT void shader_mgr_clear_cache();

		R_EXPORT u32 shader_mgr_get_vertex_elements(const shader_t& shader_id);

		R_EXPORT shader_program_t shader_mgr_create_program(const shader_program_create_desc& desc);
		R_EXPORT void shader_mgr_free_program(const shader_program_t& program_id);
		R_EXPORT void shader_mgr_clear_program_cache();

		R_EXPORT u32 shader_mgr_get_program_cache_count();
	}
}