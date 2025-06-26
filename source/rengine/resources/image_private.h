#pragma once
#include "./image.h"
#include "../io/logger.h"
#include "../strings.h"

namespace rengine {
	namespace resources {
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
			u8 data[4];
		};

		struct image_t {
			image_t* prev{ null };
			image_t* next{ null };
			math::uvec2 size{ 1, 1 };
			u8 components{ 4 };
		};

		struct image_state {
			u32 num_images{ 0 };
			image_t* root{ null };
			io::ILog* logger{ null };
		};

		static u8 g_image_fmt_components_tbl[] = {
			0, // unknown
			4, // rgba8
			4, // bgra8
			4, // rgba8_srgb
			4, // bgra8_srgb
			0, // rgba16f
			0, // rgba32f
			0, // d16
			0, // d24s8
			0, // d32s8
			0, // d32f
			1, // r8
			2, // rg8
			0, // bc1_dxt1 (RGB/A)
			0, // bc3_dxt5 (RGBA)
			0, // bc4
			0, // bc5
			0, // bc6h (HDR RGB)
			0, // bc7
			0, // max
		};

		void image__validate_pos(const image_t* img, math::uvec2& pos);

		extern image_state g_image_state;

		ptr image__malloc(size_t size);
		ptr image__realloc(ptr mem, size_t size);
		void image__free(ptr mem);

		void image__init();
		void image__deinit();
		image_t* image__alloc(const image_create_desc& desc);
		void image__free_images();
		void image__destroy(image_t* image);
		image_t* image__load(const byte* image_bin, size_t size);

		image_t* image__try_convert_rgba(image_t* image, byte default_alpha_value);
		void image__try_flip_channels(image_t* image);
	}
}