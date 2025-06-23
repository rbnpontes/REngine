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

		union image_pixel_color {
			struct {
				u8 r;
				u8 g;
				u8 b;
				u8 a;
			};
			struct {
				u32 rgba;
			};
		};

		struct image_pixel_set_desc {
			math::uvec2 pos{ 0, 0 };
			image_pixel_color color{ 0x0 };
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

		R_EXPORT void image_get_pixelbuffer(const image_t* image, ptr* pixelbuffer_out, u32* pixelbuffer_size);
		R_EXPORT void image_set_pixelbuffer(image_t* image, const ptr data);

		R_EXPORT void image_get_size(const image_t* image, math::uvec2& size);
		R_EXPORT void image_resize(image_t* image, const math::uvec2& size);

		R_EXPORT void image_set_pixel(image_t* image, const image_pixel_set_desc& desc);
                R_EXPORT void image_get_pixel(const image_t* image, const math::uvec2& pos, math::color& color);

		R_EXPORT graphics::texture_2d_t image_create_texture(const image_texture_create_desc& desc);
	}
}