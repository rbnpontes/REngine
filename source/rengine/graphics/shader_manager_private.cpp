#include "./shader_manager_private.h"
#include "./graphics_private.h"

#include "../core/string_pool.h"

namespace rengine {
	namespace graphics {
		shader_state g_shader_mgr_state = {};

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
			const auto& state = g_shader_mgr_state;
			const auto it = state.shaders.find_as(shader_id);
			if (it == state.shaders.end())
				return null;
			return it->second.handler;
		}

		void shader_mgr__free(const shader_entry& entry)
		{
			entry.handler->Release();
			core::alloc_free(entry.resources);
		}

		void shader_mgr__collect_resources(Diligent::IShader* shader, shader_resource* resources)
		{
			const auto resource_count = shader->GetResourceCount();
			for (u32 i = 0; i < resource_count; ++i) {
				Diligent::ShaderResourceDesc resource_desc;
				shader->GetResourceDesc(i, resource_desc);

				core::hash_t resource_hash = 0;
				c_str resource_name = core::string_pool_intern(resource_desc.Name, &resource_hash);
				resources[i] ={
					resource_hash,
					shader_mgr__get_resource_type(&resource_desc),
					resource_name
				};
			}
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

		shader_resource_type shader_mgr__get_resource_type(Diligent::ShaderResourceDesc* desc)
		{
			shader_resource_type res_type = g_shader_resource_tbl[desc->Type];
			if (res_type == shader_resource_type::texture && desc->ArraySize > 0)
				res_type = shader_resource_type::texarray;
			return res_type;
		}
	}
}