#include "image.h"
#include "./image_private.h"
#include "../graphics/texture_manager.h"
#include "../exceptions.h"
#include "../strings.h"
#include "../io/logger.h"
#include <cstring>

namespace rengine {
	namespace resources {
		image_t* image_create(const image_create_desc& desc)
		{
			return image__alloc(desc);
		}

		void image_destroy(image_t* image)
		{
			image__destroy(image);
		}

		void image_get_pixelbuffer(const image_t* image, byte** pixelbuffer_out, u32* pixelbuffer_size)
		{
			if (!image)
				return;

			if (pixelbuffer_out)
				*pixelbuffer_out = (byte*)image + sizeof(image_t);

			if (pixelbuffer_size)
				*pixelbuffer_size = image->size.x * image->size.y * image->components;
		}

		void image_set_pixelbuffer(image_t* image, const byte* data)
		{
			if (!image || !data)
				return;

			byte* pixelbuffer; u32 size;
			image_get_pixelbuffer(image, &pixelbuffer, &size);
			memcpy(pixelbuffer, data, size);
		}

		void image_get_size(const image_t* image, math::uvec2& size)
		{
			if (!image)
				return;
			size = image->size;
		}

		void image_resize(image_t* image, const math::uvec2& size)
		{
			(void)image; (void)size;
			throw not_implemented_exception();
		}

		void image_set_pixel(image_t* image, const math::uvec2& pos, const math::color& color)
		{
			auto b_color = math::byte_color::from(color);
			image_set_uint_pixel(image, pos, b_color.to_uint());
		}

		void image_set_uint_pixel(const image_t* image, const math::uvec2& pos, u32 color)
		{
			if (!image)
				return;

			auto pixel_pos = pos;
			image__validate_pos(image, pixel_pos);

			byte* pixelbuffer; u32 size;
			image_get_pixelbuffer(image, &pixelbuffer, &size);

			auto index = pos.y * image->size.x + pos.x;
			auto pixel = (image_pixel_color*)(pixelbuffer + index * image->components);
			image_pixel_color color_data = { color };

			for (u8 i = 0; i < image->components; ++i)
				pixel->data[i] = color_data.data[i];
		}

		u32 image_get_uint_pixel(const image_t* image, const math::uvec2& pos)
		{
			if (!image)
				return 0;

			auto p = pos;
			image__validate_pos(image, p);

			byte* pixelbuffer; u32 size;
			image_get_pixelbuffer(image, &pixelbuffer, &size);
			auto index = p.y * image->size.x + p.x;
			
			auto pixel = (image_pixel_color*)(pixelbuffer + index * image->components);
			auto result = image_pixel_color{};
			for (u8 i = 0; i < image->components; ++i)
				result.data[i] = pixel->data[i];

			return result.rgba;
		}

		math::color image_get_pixel(const image_t* image, const math::uvec2& pos)
		{
			u32 color = image_get_uint_pixel(image, pos);
			math::byte_color b_color(color);
			return b_color.to_color();
		}

		static bool image__validate_format(u8 comps, graphics::texture_format fmt)
		{
			auto fmt_idx = (u8)fmt;
			if(fmt_idx >= (u8)graphics::texture_format::bc1_dxt1 && fmt_idx <= (u8)graphics::texture_format::bc7)
				throw resources::resource_exception(strings::exceptions::g_image_invalid_compressed_format);

			auto expected_num_components = g_image_fmt_components_tbl[fmt_idx];
			return expected_num_components != 0;
		}

		graphics::texture2d_t image_create_texture(const image_texture_create_desc& desc)
		{
			if (!desc.source)
				throw null_exception(strings::exceptions::g_image_create_texture_source_null);

			auto* img = desc.source;
			if (!image__validate_format(img->components, desc.format))
				throw resources::resource_exception(strings::exceptions::g_image_invalid_format);

			byte* pixelbuffer; u32 size;
			image_get_pixelbuffer(img, &pixelbuffer, &size);

			graphics::texture_create_desc<graphics::texture_2d_size> tex_desc;
			tex_desc.name = desc.name;
			tex_desc.size = { img->size.x, img->size.y };
			tex_desc.format = desc.format;
			tex_desc.usage = desc.usage;
			tex_desc.generate_mips = desc.generate_mips;
			tex_desc.readable = desc.readable;

			graphics::texture_resource_data data{};
			data.data = pixelbuffer;
			data.stride = img->size.x * img->components;

			auto result = graphics::texture_mgr_create_tex2d(tex_desc, data);
			if(tex_desc.generate_mips)
				graphics::texture_mgr_tex2d_gen_mipmap(result);
			return result;
		}
		
		image_t* image_decode(const byte* image_bin, size_t size)
		{
			if (!image_bin || size == 0)
				return null;
			
			return image__load(image_bin, size);
		}
		
		image_t* image_convert_to_rgba(image_t* image, byte default_alpha_value)
		{
			return image__try_convert_rgba(image, default_alpha_value);
		}

		void image_flip_channels(image_t* image)
		{
			image__try_flip_channels(image);
		}
	}
}
