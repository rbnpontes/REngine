#include "./shader_manager_private.h"
#include "./graphics_private.h"

#include "../core/string_pool.h"

namespace rengine {
	namespace graphics {
		shader_state g_shader_mgr_state = {};

		void shader_mgr__deinit()
		{
			shader_mgr_clear_program_cache();
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
			ci.Desc.UseCombinedTextureSamplers = true;
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

		const shader_program* shader_mgr__get_program(const shader_program_t& program_id)
		{
			if (program_id == no_shader_program)
				return null;
			auto& state = g_shader_mgr_state;
			const auto it = state.programs.find_as(program_id);
			if (it == state.programs.end())
				return null;

			return &it->second;
		}

		shader_program_t shader_mgr__create_program(const shader_program_create_desc& desc)
		{
			auto& state = g_shader_mgr_state;
			const auto hash = shader_mgr__hash_program_desc(desc.desc);
			const auto it = state.programs.find_as(hash);

			if (it != state.programs.end())
				return hash;
			// TODO: make validation of what kind of shaders are used
			shader_entry* entries[(u8)shader_type::max] = {};
			shader_mgr__get_entries_batch(reinterpret_cast<const shader_t*>(&desc.desc), entries);

			shader_program program;
			program.desc = desc.desc;

			// collect shader resources and insert into program
			for (u8 i = 0; i < (u8)shader_type::max; ++i) {
				if (!entries[i])
					continue;

				for (u8 j = 0; j < entries[i]->num_resources; ++j) {
					auto& res = entries[i]->resources[j];
					if (resource_type::unknow == res.type)
						continue;
					auto it = program.resources.find_as(res.id);
					if (program.resources.end() != it) {
						it->second.shader_flags |= 1 << i;
						continue;
					}

					program.resources[res.id] = res;
					program.num_resources++;
				}
			}

			// finally, insert program
			state.programs[hash] = program;
			state.programs_count++;
			return hash;
		}

		void shader_mgr__free(const shader_entry& entry)
		{
			entry.handler->Release();
			core::alloc_free(entry.resources);
		}

		void shader_mgr__collect_resources(Diligent::IShader* shader, shader_resource* resources, u32 shader_type_flag)
		{
			const auto resource_count = shader->GetResourceCount();
			for (u32 i = 0; i < resource_count; ++i) {
				Diligent::ShaderResourceDesc resource_desc;
				shader->GetResourceDesc(i, resource_desc);

				core::hash_t resource_hash = 0;
				c_str resource_name = core::string_pool_intern(resource_desc.Name, &resource_hash);
				resources[i] = {
					resource_hash,
					shader_mgr__get_resource_type(&resource_desc),
					resource_name,
					shader_type_flag
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

		resource_type shader_mgr__get_resource_type(Diligent::ShaderResourceDesc* desc)
		{
			resource_type res_type = g_shader_resource_tbl[desc->Type];
			if (res_type == resource_type::tex2d && desc->ArraySize > 1)
				res_type = resource_type::texarray;
			return res_type;
		}

		void shader_mgr__get_entries_batch(const shader_t* shaders, shader_entry** entries_output)
		{
			// there's no limit check here, we assume that the caller
			// always send the correct size 
			for (u8 i = 0; i < (u8)shader_type::max; ++i) {
				if (shaders[i] == no_shader) {
					entries_output[i] = null;
					continue;
				}

				auto it = g_shader_mgr_state.shaders.find_as(shaders[i]);
				if (it == g_shader_mgr_state.shaders.end())
					entries_output[i] = null;
				else
					entries_output[i] = &it->second;
			}
		}

		core::hash_t shader_mgr__hash_desc(const shader_create_desc& desc)
		{
			core::hash_t result = core::hash(desc.name);
			result = core::hash_combine(result, (core::hash_t)desc.type);
			result = core::hash_combine(result, core::hash(desc.name));
			result = core::hash_combine(result, desc.source_code_length);
			result = core::hash_combine(result, core::hash(desc.bytecode, desc.bytecode_length));
			result = core::hash_combine(result, desc.bytecode_length);
			result = core::hash_combine(result, desc.vertex_elements);
			result = core::hash_combine(result, desc.num_macros);
			for (u32 i = 0; i < desc.num_macros; ++i) {
				const auto& macro = desc.macros[i];
				result = core::hash_combine(result, core::hash(macro.name));
				result = core::hash_combine(result, core::hash(macro.definition));
			}
			return result;
		}

		core::hash_t shader_mgr__hash_program_desc(const shader_program_desc& desc)
		{
			return core::hash_combine(desc.vertex_shader, desc.pixel_shader);
		}
	}
}