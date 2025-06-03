#pragma once
#include "./texture_manager.h"

#include "../core/pool.h"
#include "../io/logger.h"

#include "Texture.h"

namespace rengine {
	namespace graphics {
		struct texture_entry {
			Diligent::ITexture* handler{ null };
		};

		struct texture_manager_state {
			core::array_pool<texture_entry, GRAPHICS_MAX_ALLOC_TEX2D> textures_2d{};
			core::array_pool<texture_entry, GRAPHICS_MAX_ALLOC_TEX3D> textures_3d{};
			core::array_pool<texture_entry, GRAPHICS_MAX_ALLOC_TEXCUBE> textures_cube{};
			core::array_pool<texture_entry, GRAPHICS_MAX_ALLOC_TEXARRAY> textures_array{};

			texture_2d_t white_dummy_tex2d{ no_texture_2d };
			io::ILog* log{ null };
		};

		static Diligent::TEXTURE_FORMAT g_texture_mgr_formats[] = {
			Diligent::TEX_FORMAT_UNKNOWN,
			Diligent::TEX_FORMAT_RGBA8_UNORM,
			Diligent::TEX_FORMAT_BGRA8_UNORM,
			Diligent::TEX_FORMAT_RGBA8_UNORM_SRGB,
			Diligent::TEX_FORMAT_BGRA8_UNORM_SRGB,
			Diligent::TEX_FORMAT_RGBA16_FLOAT,
			Diligent::TEX_FORMAT_RGBA32_FLOAT,
			Diligent::TEX_FORMAT_D16_UNORM,
			Diligent::TEX_FORMAT_D24_UNORM_S8_UINT,
			Diligent::TEX_FORMAT_D32_FLOAT_S8X24_UINT,
			Diligent::TEX_FORMAT_D32_FLOAT,
			Diligent::TEX_FORMAT_R8_UNORM,
			Diligent::TEX_FORMAT_RG8_UNORM,
			Diligent::TEX_FORMAT_BC1_UNORM,
			Diligent::TEX_FORMAT_BC3_UNORM,
			Diligent::TEX_FORMAT_BC4_UNORM,
			Diligent::TEX_FORMAT_BC5_UNORM,
			Diligent::TEX_FORMAT_BC6H_UF16,
			Diligent::TEX_FORMAT_BC7_UNORM,
		};

		extern texture_manager_state g_texture_mgr_state;

		void texture_mgr__init();
		void texture_mgr__deinit();

		void texture_mgr__init_dummy_white_tex2d();

		void texture_mgr__fill_tex2d_desc(const texture_create_desc<texture_2d_size>& desc, Diligent::TextureDesc& out_desc);
		void texture_mgr__fill_subres(const texture_data_desc& data, Diligent::TextureSubResData* subres);

		Diligent::ITexture* texture_mgr__create(Diligent::TextureDesc& desc, Diligent::TextureData& data, bool gen_mipmap);
	}
}