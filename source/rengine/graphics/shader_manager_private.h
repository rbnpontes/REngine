#pragma once
#include "../base_private.h"
#include "./shader_manager.h"

#include <Shader.h>

namespace rengine {
	namespace graphics {
		struct shader_program {
			hash_map<core::hash_t, shader_resource> resources{};
			u32 num_resources{ 0 };
			shader_program_desc desc{};
		};

		struct shader_entry {
			Diligent::IShader* handler{ null };
			u32 num_resources{ 0 };
			shader_resource* resources{ null };
		};

		struct shader_state {
			hash_map<shader_t, shader_entry> shaders{};
			u32 shaders_count{};

			hash_map<shader_t, shader_program> programs{};
			u32 programs_count{};
		};

		extern shader_state g_shader_mgr_state;

		static constexpr Diligent::SHADER_TYPE g_shader_type_tbl[] = {
			Diligent::SHADER_TYPE_VERTEX,
			Diligent::SHADER_TYPE_PIXEL,
			Diligent::SHADER_TYPE_UNKNOWN
		};
		static constexpr resource_type g_shader_resource_tbl[] = {
			resource_type::unknow,
			//SHADER_RESOURCE_TYPE_CONSTANT_BUFFER
			resource_type::cbuffer,
			//SHADER_RESOURCE_TYPE_TEXTURE_SRV
			resource_type::tex2d,
			//SHADER_RESOURCE_TYPE_BUFFER_SRV
			resource_type::unknow,
			//SHADER_RESOURCE_TYPE_TEXTURE_UAV
			resource_type::unknow,
			//SHADER_RESOURCE_TYPE_BUFFER_UAV
			resource_type::unknow,
			//SHADER_RESOURCE_TYPE_SAMPLER
			resource_type::unknow,
			//SHADER_RESOURCE_TYPE_INPUT_ATTACHMENT
			resource_type::unknow,
			//SHADER_RESOURCE_TYPE_ACCEL_STRUCT
			resource_type::unknow
		};

		void shader_mgr__deinit();

		Diligent::IShader* shader_mgr__create_shader(const shader_create_desc& desc);
		Diligent::IShader* shader_mgr__get_handle(const shader_t& shader_id);
		void shader_mgr__get_program(const shader_program_t& program_id, shader_program* program_output);

		shader_program_t shader_mgr__create_program(const shader_program_create_desc& desc);

		void shader_mgr__free(const shader_entry& entry);
		void shader_mgr__collect_resources(Diligent::IShader* shader, shader_resource* resources);
		void shader_mgr__fill_vertex_elements_macros(vector<Diligent::ShaderMacro>& macros, u32 elements);

		resource_type shader_mgr__get_resource_type(Diligent::ShaderResourceDesc* desc);

		void shader_mgr__get_entries_batch(const shader_t* shaders, shader_entry** entries_output);

		core::hash_t shader_mgr__hash_desc(const shader_create_desc& desc);
		core::hash_t shader_mgr__hash_program_desc(const shader_program_desc& desc);
	}
}