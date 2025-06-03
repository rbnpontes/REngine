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

			if(desc.type == shader_type::vertex && desc.vertex_elements != 0)
				shader_mgr__fill_vertex_elements_macros(macros, desc.vertex_elements);

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
			return shader;
		}

		Diligent::IShader* shader_mgr__get_handle(const shader_t& shader_id)
		{
			const auto it = g_shader_tbl.find_as(shader_id);
			if (it == g_shader_tbl.end())
				return null;
			return it->second;
		}

		void shader_mgr__fill_vertex_elements_macros(vector<Diligent::ShaderMacro>& macros, u32 elements)
		{
			const auto attrib_names = strings::graphics::shaders::g_attrib_names;
			if (elements & (u32)vertex_elements::position) {
				const auto pair = attrib_names[VERTEX_ELEMENT_POSITION_IDX];
				macros.push_back({ pair[0], pair[1] });
			}

			if (elements & (u32)vertex_elements::normal) {
				const auto pair = attrib_names[VERTEX_ELEMENT_NORMAL_IDX];
				macros.push_back({ pair[0], pair[1] });
			}

			if (elements & (u32)vertex_elements::tangent) {
				const auto pair = attrib_names[VERTEX_ELEMENT_TANGENT_IDX];
				macros.push_back({ pair[0], pair[1] });
			}

			if (elements & (u32)vertex_elements::color) {
				const auto pair = attrib_names[VERTEX_ELEMENT_COLOR_IDX];
				macros.push_back({ pair[0], pair[1] });
			}

			if (elements & (u32)vertex_elements::colorf) {
				const auto pair = attrib_names[VERTEX_ELEMENT_COLORF_IDX];
				macros.push_back({ pair[0], pair[1] });
			}

			if (elements & (u32)vertex_elements::uv) {
				const auto pair = attrib_names[VERTEX_ELEMENT_UV_IDX];
				macros.push_back({ pair[0], pair[1] });
			}

			if (elements & (u32)vertex_elements::instancing) {
				const auto pair = attrib_names[VERTEX_ELEMENT_INSTANCING_IDX];
				macros.push_back({ pair[0], pair[1] });
			}
		}
	}
}