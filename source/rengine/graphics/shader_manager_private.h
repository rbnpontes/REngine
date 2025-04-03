#pragma once
#include "../base_private.h"
#include "./shader_manager.h"

#include <Shader.h>

namespace rengine {
	namespace graphics {
		extern hash_map<shader_t, Diligent::IShader*> g_shader_tbl;
		extern u32 g_shader_count;

		static constexpr Diligent::SHADER_TYPE g_shader_type_tbl[] = {
			Diligent::SHADER_TYPE_VERTEX,
			Diligent::SHADER_TYPE_PIXEL,
			Diligent::SHADER_TYPE_UNKNOWN
		};

		Diligent::IShader* shader_mgr__create_shader(const shader_create_desc& desc);
		Diligent::IShader* shader_mgr__get_handle(const shader_t& shader_id);
	}
}