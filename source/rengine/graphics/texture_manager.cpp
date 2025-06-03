#include "./texture_manager.h"
#include "./texture_manager_private.h"
#include "./buffer_manager_private.h"
#include "./graphics_private.h"

namespace rengine {
	namespace graphics {
		texture_2d_t texture_mgr_create_tex2d(const texture_create_desc<texture_2d_size>& desc)
		{
			texture_data_desc data_desc{};
			return texture_mgr_create_tex2d(desc, data_desc);
		}
		texture_2d_t texture_mgr_create_tex2d(const texture_create_desc<texture_2d_size>& desc, const texture_resource_data& data)
		{
			texture_data_desc data_desc;
			data_desc.num_resources = 1;
			data_desc.resources = const_cast<texture_resource_data*>(&data);
			return texture_mgr_create_tex2d(desc, data_desc);
		}
		texture_2d_t texture_mgr_create_tex2d(const texture_create_desc<texture_2d_size>& desc, const texture_data_desc& data)
		{
			auto& state = g_texture_mgr_state;
			auto device = g_graphics_state.device;
			
			Diligent::TextureDesc tex_desc;
			Diligent::TextureData tex_data;
			texture_mgr__fill_tex2d_desc(desc, tex_desc);

			vector<Diligent::TextureSubResData> subres(data.num_resources);
			tex_data.NumSubresources = data.num_resources;
			tex_data.pSubResources = subres.data();
			texture_mgr__fill_subres(data, subres.data());

			Diligent::ITexture* result = texture_mgr__create(tex_desc, tex_data, desc.generate_mips);
			
			texture_entry entry = {
				result
			};
			return state.textures_2d.push_back(entry);
		}

		texture_3d_t texture_mgr_create_tex3d(const texture_create_desc<texture_3d_size>& desc)
		{
			texture_data_desc data_desc{};
			return texture_mgr_create_tex3d(desc, data_desc);
		}
		texture_3d_t texture_mgr_create_tex3d(const texture_create_desc<texture_3d_size>& desc, const texture_resource_data& data)
		{
			texture_data_desc data_desc;
			data_desc.num_resources = 1;
			data_desc.resources = const_cast<texture_resource_data*>(&data);
			return texture_mgr_create_tex3d(desc, data_desc);
		}
		texture_3d_t texture_mgr_create_tex3d(const texture_create_desc<texture_3d_size>& desc, const texture_data_desc& data)
		{
			throw not_implemented_exception();
		}

		texture_cube_t texture_mgr_create_tex_cube(const texture_create_desc<texture_cube_size>& desc)
		{
			texture_data_desc data_desc{};
			return texture_mgr_create_tex_cube(desc, data_desc);
		}
		texture_cube_t texture_mgr_create_tex_cube(const texture_create_desc<texture_cube_size>& desc, const texture_resource_data& data)
		{
			texture_data_desc data_desc;
			data_desc.num_resources = 1;
			data_desc.resources = const_cast<texture_resource_data*>(&data);
			return texture_mgr_create_tex_cube(desc, data_desc);
		}
		texture_cube_t texture_mgr_create_tex_cube(const texture_create_desc<texture_cube_size>& desc, const texture_data_desc& data)
		{
			throw not_implemented_exception();
		}

		texture_array_t texture_mgr_create_tex_array(const texture_create_desc<texture_array_size>& desc)
		{
			texture_data_desc data_desc{};
			return texture_mgr_create_tex_array(desc, data_desc);
		}
		texture_array_t texture_mgr_create_tex_array(const texture_create_desc<texture_array_size>& desc, const texture_resource_data& data)
		{
			texture_data_desc data_desc;
			data_desc.num_resources = 1;
			data_desc.resources = const_cast<texture_resource_data*>(&data);
			return texture_mgr_create_tex_array(desc, data_desc);
		}
		texture_array_t texture_mgr_create_tex_array(const texture_create_desc<texture_array_size>& desc, const texture_data_desc& data)
		{
			throw not_implemented_exception();
		}

		void texture_mgr_destroy_tex2d(texture_2d_t id)
		{
			auto& state = g_texture_mgr_state;
			auto& entry = state.textures_2d[id];
			entry.value.handler->Release();
			state.textures_2d.erase(id);
		}
		void texture_mgr_destroy_tex3d(texture_3d_t id)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_destroy_texcube(texture_cube_t id)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_destroy_texarray(texture_array_t id)
		{
			throw not_implemented_exception();
		}

		void texture_mgr_resize(texture_2d_t id, const math::vec2& size)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_resize(texture_3d_t id, const math::vec3& size)
		{
			throw not_implemented_exception();
		}

		void texture_mgr_get_desc(texture_2d_t id, texture_create_desc<texture_2d_size>* desc)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_get_desc(texture_3d_t id, texture_create_desc<texture_3d_size>* desc)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_get_desc(texture_cube_t id, texture_create_desc<texture_cube_size>* desc)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_get_desc(texture_array_t id, texture_create_desc<texture_array_size>* desc)
		{
			throw not_implemented_exception();
		}

		void texture_mgr_tex2d_update(texture_2d_t id, const texture2d_update_desc& desc)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_tex3d_update(texture_3d_t id, const texture3d_update_desc& desc)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_texcube_update(texture_cube_t id, const texturecube_update_desc& desc)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_texarray_update(texture_array_t id, const texturearray_update_desc& desc)
		{
			throw not_implemented_exception();
		}

		ptr texture_mgr_tex2d_map(texture_2d_t id, u32 mip_level)
		{
			throw not_implemented_exception();
		}
		ptr texture_mgr_tex3d_map(texture_3d_t id, u32 mip_level)
		{
			throw not_implemented_exception();
		}
		ptr texture_mgr_texcube_map(texture_cube_t id, u32 mip_level, u32 array_slice)
		{
			throw not_implemented_exception();
		}
		ptr texture_mgr_texarray_map(texture_array_t id, u32 mip_level, u32 array_slice)
		{
			throw not_implemented_exception();
		}

		void texture_mgr_tex2d_unmap(texture_2d_t id)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_tex3d_unmap(texture_2d_t id)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_texcube_unmap(texture_cube_t id)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_texarray_unmap(texture_array_t id)
		{
			throw not_implemented_exception();
		}

		void texture_mgr_tex2d_copy(const texture2d_copy_desc& desc)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_tex3d_copy(const texture3d_copy_desc& desc)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_texcube_copy(const texturecube_copy_desc& desc)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_texarray_copy(const texturearray_copy_desc& desc)
		{
			throw not_implemented_exception();
		}

		void texture_mgr_tex2d_gen_mipmap(texture_2d_t id)
		{
			auto& state = g_texture_mgr_state;
			auto& entry = state.textures_2d[id];
			auto ctx = g_graphics_state.contexts[0];
			auto view = entry.value.handler->GetDefaultView(Diligent::TEXTURE_VIEW_SHADER_RESOURCE);

			ctx->GenerateMips(view);
		}
		void texture_mgr_tex3d_gen_mipmap(texture_3d_t id)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_texcube_gen_mipmap(texture_cube_t id)
		{
			throw not_implemented_exception();
		}
		void texture_mgr_texarray_gen_mipmap(texture_cube_t id)
		{
			throw not_implemented_exception();
		}
		
		texture_2d_t texture_mgr_get_white_dummy_tex2d()
		{
			return g_texture_mgr_state.white_dummy_tex2d;
		}
	}
}