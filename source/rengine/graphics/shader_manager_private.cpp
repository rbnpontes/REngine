#include "./shader_manager_private.h"
#include "./graphics_private.h"

namespace rengine {
	namespace graphics {
		hash_map<shader_t, Diligent::IShader*> g_shader_tbl = {};
		u32 g_shader_count = 0;

		void shader_mgr__deinit()
		{
			shader_mgr_clear_cache();
		}

		Diligent::IShader* shader_mgr__create_shader(const shader_create_desc& desc)
		{
			using namespace Diligent;
			const auto device = g_graphics_state.device;

			vector<ShaderMacro> macros(desc.num_macros);
			for (u32 i = 0; i < desc.num_macros; ++i) {
				macros[i].Name = desc.macros[i].name;
				macros[i].Definition = desc.macros[i].definition;
			}

			ShaderCreateInfo ci = {};
			ci.Desc.Name = desc.name;
			ci.Desc.ShaderType = g_shader_type_tbl[(u8)desc.type];
			ci.SourceLanguage = SHADER_SOURCE_LANGUAGE_HLSL;
			ci.Source = desc.source_code;
			ci.SourceLength = desc.source_code_length;
			ci.ByteCode = desc.bytecode;
			ci.ByteCodeSize = desc.bytecode_length;
			ci.EntryPoint = strings::graphics::g_shader_entrypoint;
			ci.Macros.Elements = macros.data();
			ci.Macros.Count = macros.size();

			IShader* shader = null;
			device->CreateShader(ci, &shader, null);
			if (shader)
				shader->AddRef();
			return shader;
		}

		Diligent::IShader* shader_mgr__get_handle(const shader_t& shader_id)
		{
			const auto it = g_shader_tbl.find_as(shader_id);
			if (it == g_shader_tbl.end())
				return null;
			return it->second;
		}
	}
}