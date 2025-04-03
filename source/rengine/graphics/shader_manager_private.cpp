#include "./shader_manager_private.h"
#include "./graphics_private.h"

namespace rengine {
	namespace graphics {
		hash_map<shader_t, Diligent::IShader*> g_shader_tbl = {};
		u32 g_shader_count = 0;

		Diligent::IShader* shader_mgr__create_shader(const shader_create_desc& desc)
		{
			using namespace Diligent;
			const auto device = g_graphics_state.device;
			ShaderCreateInfo ci = {};
			ci.Desc.Name = desc.name;
			ci.Desc.ShaderType = g_shader_type_tbl[(u8)desc.type];
			ci.SourceLanguage = SHADER_SOURCE_LANGUAGE_HLSL;
			ci.Source = desc.source_code;
			ci.SourceLength = desc.source_code_length;
			ci.ByteCode = desc.bytecode;
			ci.ByteCodeSize = desc.bytecode_length;
			ci.EntryPoint = "main";

			IShader* shader;
			device->CreateShader(ci, &shader, null);
			if (shader)
				shader->AddRef();
			return shader;
		}
	}
}