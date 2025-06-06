#pragma once
#include "../base_private.h"
#include "./shader_manager.h"

#include <Shader.h>

namespace rengine {
	namespace graphics {
		struct shader_entry {
			Diligent::IShader* handler{ null };
			u32 num_resources{ 0 };
			shader_resource* resources{ null };
		};

		struct shader_state {
			hash_map<shader_t, shader_entry> shaders{};
			u32 shaders_count{};
		};

		extern shader_state g_shader_mgr_state;

		static constexpr Diligent::SHADER_TYPE g_shader_type_tbl[] = {
			Diligent::SHADER_TYPE_VERTEX,
			Diligent::SHADER_TYPE_PIXEL,
			Diligent::SHADER_TYPE_UNKNOWN
		};
		static constexpr shader_resource_type g_shader_resource_tbl[] = {
			shader_resource_type::unknow,
			//SHADER_RESOURCE_TYPE_CONSTANT_BUFFER
			shader_resource_type::cbuffer,
			//SHADER_RESOURCE_TYPE_TEXTURE_SRV
			shader_resource_type::texture,
			//SHADER_RESOURCE_TYPE_BUFFER_SRV
			shader_resource_type::unknow,
			//SHADER_RESOURCE_TYPE_TEXTURE_UAV
			shader_resource_type::unknow,
			//SHADER_RESOURCE_TYPE_BUFFER_UAV
			shader_resource_type::unknow,
			//SHADER_RESOURCE_TYPE_SAMPLER
			shader_resource_type::unknow,
			//SHADER_RESOURCE_TYPE_INPUT_ATTACHMENT
			shader_resource_type::unknow,
			//SHADER_RESOURCE_TYPE_ACCEL_STRUCT
			shader_resource_type::unknow
		};

		void shader_mgr__deinit();

		Diligent::IShader* shader_mgr__create_shader(const shader_create_desc& desc);
		Diligent::IShader* shader_mgr__get_handle(const shader_t& shader_id);

		void shader_mgr__free(const shader_entry& entry);
		void shader_mgr__collect_resources(Diligent::IShader* shader, shader_resource* resources);
		void shader_mgr__fill_vertex_elements_macros(vector<Diligent::ShaderMacro>& macros, u32 elements);

		shader_resource_type shader_mgr__get_resource_type(Diligent::ShaderResourceDesc* desc);
	}
}