#pragma once
#include <rengine/api.h>
#include <rengine/types.h>
#include <rengine/math/math-types.h>

namespace rengine {
	namespace resources {
		struct image_t;

		struct image_create_desc {
			math::uvec2 size{ 1, 1 };
			u8 components{ 4 }; // 1 = gray, 2 = gray + alpha, 3 = rgb | bgr, 4 = rgba | bgra
		};

		struct image_pixel_set_desc {
			math::uvec2 pos{ 0, 0 };
			math::color color{ math::color::white };
		};

		struct image_texture_create_desc {
			c_str name;
			image_t* source{ null };
			graphics::texture_format format{ graphics::texture_format::rgba8 };
			graphics::resource_usage usage{ graphics::resource_usage::immutable };
			bool generate_mips{ false };
			bool readable{ false };
		};

		R_EXPORT image_t* image_create(const image_create_desc& desc);
		R_EXPORT void image_destroy(image_t* image);

		R_EXPORT void image_get_pixelbuffer(const image_t* image, byte** pixelbuffer_out, u32* pixelbuffer_size);
		R_EXPORT void image_set_pixelbuffer(image_t* image, const byte* data);

		R_EXPORT void image_get_size(const image_t* image, math::uvec2& size);
		R_EXPORT void image_resize(image_t* image, const math::uvec2& size);

		R_EXPORT void image_set_pixel(const image_t* image, const math::uvec2& pos, const math::color& color);
		R_EXPORT void image_set_uint_pixel(const image_t* image, const math::uvec2& pos, u32 color);
		R_EXPORT math::color image_get_pixel(const image_t* image, const math::uvec2& pos);
		R_EXPORT u32 image_get_uint_pixel(const image_t* image, const math::uvec2& pos);

		R_EXPORT graphics::texture2d_t image_create_texture(const image_texture_create_desc& desc);

		// decode image from binary data. Supported types: PNG, JPEG, BMP, TGA, GIF, HDR
		R_EXPORT image_t* image_decode(const byte* image_bin, size_t size);

		// convert an non-rgba image to rgba
		// if image is already rgba, it will return the same image
		// otherwise it will create a new image with rgba format
		R_EXPORT image_t* image_convert_to_rgba(image_t* image, byte default_alpha_value = 0xFF);
		// flip image channels from RGBA to BGRA or vice versa
		R_EXPORT void image_flip_channels(image_t* image);
	}
}