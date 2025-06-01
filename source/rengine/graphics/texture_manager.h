#pragma once
#include <rengine/api.h>
#include <rengine/math/math-types.h>
#include <rengine/math/vec3.h>

namespace rengine {
	namespace graphics {
		struct texture_resource_data {
			/// <summary>
			/// Data where the pixelbuffer/pixel array will be copied
			/// </summary>
			ptr data{ null };
			/// <summary>
			/// Same as data param, but an GPU buffer will be
			/// used as source of data
			/// </summary>
			u32 src_buffer{ UINT32_MAX };
			u64 src_offset{ 0 };
			u64 stride{ 0 };
			u64 depth_stride{ 0 };
		};

		struct texture_data_desc {
			texture_resource_data* resources;
			u32 num_resources{ 0 };
		};

		struct texture_2d_size {
			u32 width{ 0 };
			u32 height{ 0 };
		};
		struct texture_3d_size {
			u32 width{ 0 };
			u32 height{ 0 };
			u32 depth{ 0 };
		};
		struct texture_array_size {
			u32 width{ 0 };
			u32 height{ 0 };
			u32 array_size{ 0 };
		};
		struct texture_cube_size {
			u32 width{ 0 };
			u32 height{ 0 };
			u32 array_size{ 0 };
		};

		template<typename texture_size>
		struct texture_create_desc {
			c_str name{ null };
			texture_size size{};
			texture_format format{ texture_format::rgba8 };
			u32 mip_levels{ 1 };
			resource_usage usage{ resource_usage::immutable };
			bool generate_mips{ false };
			bool readable{ false };
		};

		struct texture2d_update_desc {
			texture_2d_t id{ 0 };
			u32 mip_level{ 0 };
			ptr data{ null };
		};
		struct texture3d_update_desc {
			texture_3d_t id{ 0 };
			u32 mip_level{ 0 };
			ptr data{ null };
		};
		struct texturecube_update_desc {
			texture_cube_t id{ 0 };
			u32 mip_level{ 0 };
			u32 array_slice{ 0 };
			ptr data{ null };
		};
		struct texturearray_update_desc {
			texture_array_t id{ 0 };
			u32 mip_level{ 0 };
			u32 array_slice{ 0 };
			ptr data{ null };
		};

		struct texture2d_copy_desc {
			texture_2d_t dst_id{ 0 };
			texture_2d_t src_id{ 0 };
			u32 dst_mip_level{ 0 };
			u32 src_mip_level{ 0 };
		};
		struct texture3d_copy_desc {
			texture_3d_t dst_id{ 0 };
			texture_3d_t src_id{ 0 };
			u32 dst_mip_level{ 0 };
			u32 src_mip_level{ 0 };
		};
		struct texturecube_copy_desc {
			texture_2d_t dst_id{ 0 };
			texture_2d_t src_id{ 0 };
			u32 dst_mip_level{ 0 };
			u32 src_mip_level{ 0 };
			u32 dst_array_slice{ 0 };
			u32 src_array_slice{ 0 };
		};
		struct texturearray_copy_desc {
			texture_2d_t dst_id{ 0 };
			texture_2d_t src_id{ 0 };
			u32 dst_mip_level{ 0 };
			u32 src_mip_level{ 0 };
			u32 dst_array_slice{ 0 };
			u32 src_array_slice{ 0 };
		};

		R_EXPORT texture_2d_t texture_mgr_create_tex2d(const texture_create_desc<texture_2d_size>& desc);
		R_EXPORT texture_2d_t texture_mgr_create_tex2d(const texture_create_desc<texture_2d_size>& desc, const texture_resource_data& data);
		R_EXPORT texture_2d_t texture_mgr_create_tex2d(const texture_create_desc<texture_2d_size>& desc, const texture_data_desc& data);

		R_EXPORT texture_3d_t texture_mgr_create_tex3d(const texture_create_desc<texture_3d_size>& desc);
		R_EXPORT texture_3d_t texture_mgr_create_tex3d(const texture_create_desc<texture_3d_size>& desc, const texture_resource_data& data);
		R_EXPORT texture_3d_t texture_mgr_create_tex3d(const texture_create_desc<texture_3d_size>& desc, const texture_data_desc& data);

		R_EXPORT texture_cube_t texture_mgr_create_tex_cube(const texture_create_desc<texture_cube_size>& desc);
		R_EXPORT texture_cube_t texture_mgr_create_tex_cube(const texture_create_desc<texture_cube_size>& desc, const texture_resource_data& data);
		R_EXPORT texture_cube_t texture_mgr_create_tex_cube(const texture_create_desc<texture_cube_size>& desc, const texture_data_desc& data);

		R_EXPORT texture_array_t texture_mgr_create_tex_array(const texture_create_desc<texture_array_size>& desc);
		R_EXPORT texture_array_t texture_mgr_create_tex_array(const texture_create_desc<texture_array_size>& desc, const texture_resource_data& data);
		R_EXPORT texture_array_t texture_mgr_create_tex_array(const texture_create_desc<texture_array_size>& desc, const texture_data_desc& data);
	
		R_EXPORT void texture_mgr_destroy_tex2d(texture_2d_t id);
		R_EXPORT void texture_mgr_destroy_tex3d(texture_3d_t id);
		R_EXPORT void texture_mgr_destroy_texcube(texture_cube_t id);
		R_EXPORT void texture_mgr_destroy_texarray(texture_array_t id);

		R_EXPORT void texture_mgr_resize(texture_2d_t id, const math::vec2& size);
		R_EXPORT void texture_mgr_resize(texture_3d_t id, const math::vec3& size);

		R_EXPORT void texture_mgr_get_desc(texture_2d_t id, texture_create_desc<texture_2d_size>* desc);
		R_EXPORT void texture_mgr_get_desc(texture_3d_t id, texture_create_desc<texture_3d_size>* desc);
		R_EXPORT void texture_mgr_get_desc(texture_cube_t id, texture_create_desc<texture_cube_size>* desc);
		R_EXPORT void texture_mgr_get_desc(texture_array_t id, texture_create_desc<texture_array_size>* desc);

		R_EXPORT void texture_mgr_tex2d_update(texture_2d_t id, const texture2d_update_desc& desc);
		R_EXPORT void texture_mgr_tex3d_update(texture_3d_t id, const texture3d_update_desc& desc);
		R_EXPORT void texture_mgr_texcube_update(texture_cube_t id, const texturecube_update_desc& desc);
		R_EXPORT void texture_mgr_texarray_update(texture_array_t id, const texturearray_update_desc& desc);

		R_EXPORT ptr texture_mgr_tex2d_map(texture_2d_t id, u32 mip_level = 0);
		R_EXPORT ptr texture_mgr_tex3d_map(texture_3d_t id, u32 mip_level = 0);
		R_EXPORT ptr texture_mgr_texcube_map(texture_cube_t id, u32 mip_level = 0, u32 array_slice = 0);
		R_EXPORT ptr texture_mgr_texarray_map(texture_array_t id, u32 mip_level = 0, u32 array_slice = 0);
		
		R_EXPORT void texture_mgr_tex2d_unmap(texture_2d_t id);
		R_EXPORT void texture_mgr_tex3d_unmap(texture_2d_t id);
		R_EXPORT void texture_mgr_texcube_unmap(texture_cube_t id);
		R_EXPORT void texture_mgr_texarray_unmap(texture_array_t id);

		R_EXPORT void texture_mgr_tex2d_copy(const texture2d_copy_desc& desc);
		R_EXPORT void texture_mgr_tex3d_copy(const texture3d_copy_desc& desc);
		R_EXPORT void texture_mgr_texcube_copy(const texturecube_copy_desc& desc);
		R_EXPORT void texture_mgr_texarray_copy(const texturearray_copy_desc& desc);

		R_EXPORT void texture_mgr_tex2d_gen_mipmap(texture_2d_t id);
		R_EXPORT void texture_mgr_tex3d_gen_mipmap(texture_3d_t id);
		R_EXPORT void texture_mgr_texcube_gen_mipmap(texture_cube_t id);
		R_EXPORT void texture_mgr_texarray_gen_mipmap(texture_cube_t id);
	}
}