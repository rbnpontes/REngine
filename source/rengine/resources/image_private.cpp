#include "image_private.h"
#include "../core/allocator.h"
#include "../io/logger.h"
#include "../strings.h"

#include <fmt/format.h>

#define STB_IMAGE_IMPLEMENTATION
#define STBI_MALLOC rengine::resources::image__malloc
#define STBI_REALLOC rengine::resources::image__realloc
#define STBI_FREE rengine::resources::image__free
#include <stb/stb_image.h>

namespace rengine {
	namespace resources {
		image_state g_image_state = {};
		// TODO: add memory reuse for images
		ptr image__malloc(size_t size)
		{
			return core::alloc(size);
		}

		ptr image__realloc(ptr mem, size_t size)
		{
			return core::alloc_realloc(mem, size);
		}

		void image__free(ptr mem)
		{
			core::alloc_free(mem);
		}

		void image__init()
		{
			g_image_state = {};
			g_image_state.logger = io::logger_use(strings::logs::g_image_tag);
		}

		void image__deinit()
		{
			image__free_images();
			g_image_state = {};
		}

		void image__validate_pos(const image_t* img, math::uvec2& pos)
		{
			if (!img)
				return;

			if (pos.x >= img->size.x || pos.y >= img->size.y) {
				const auto log = g_image_state.logger;
				if (log) {
					log->warn(
						fmt::format(strings::logs::g_image_pos_exceeds_bounds,
							pos.x, pos.y, img->size.x, img->size.y)
						.c_str());
				}
				pos.x = math::min<u32>(pos.x, img->size.x - 1);
				pos.y = math::min<u32>(pos.y, img->size.y - 1);
			}
		}

		image_t* image__alloc(const image_create_desc& desc)
		{
			auto& state = g_image_state;
			auto pixelbuffer_size = desc.size.x * desc.size.y * desc.components;

			image_t* img = (image_t*)core::alloc(sizeof(image_t) + pixelbuffer_size);
			img->prev = state.root;
			img->next = null;
			img->size = desc.size;
			img->components = desc.components;
			if (state.root)
				state.root->next = img;
			state.root = img;
			state.num_images++;
			return img;
		}

		void image__free_images()
		{
			auto& state = g_image_state;
			image_t* current = state.root;
			while (current) {
				image_t* next = current->prev;
				image_destroy(current);
				current = next;
			}

			state.root = null;
			state.num_images = 0;
		}

		void image__destroy(image_t* image)
		{
			auto& state = g_image_state;
			if (!image)
				return;

			auto prev = image->prev;
			auto next = image->next;
			core::alloc_free(image);

			if (prev)
				prev->next = next;
			if (next)
				next->prev = prev;

			if (state.root == image)
				state.root = next;
			if (!state.root)
				state.root = prev;

			state.num_images--;
		}

		image_t* image__load(const byte* image_bin, size_t size)
		{
			auto logger = g_image_state.logger;
			int width, height, num_components;

			auto stbi = stbi_load_from_memory(image_bin, (int)size, &width, &height, &num_components, 0);
			if (!stbi) {
				logger->warn(strings::logs::g_image_failed_2_load);
				return null;
			}

			auto img = image_create({
				.size = math::uvec2(width, height),
				.components = (u8)num_components,
			});
			image_set_pixelbuffer(img, stbi);
			
			stbi_image_free(stbi);
			return img;
		}

		image_t* image__try_convert_rgba(image_t* image, byte default_alpha_value)
		{
			if (!image)
				return image;

			auto copy_img = image_create({
				.size = image->size,
				.components = 4
				});

			byte* dst_pixelbuffer;
			byte* src_pixelbuffer;
			u32 size = 0;

			image_get_pixelbuffer(copy_img, &dst_pixelbuffer, null);
			image_get_pixelbuffer(image, &src_pixelbuffer, &size);

			for (u32 i = 0; i < size; i += image->components) {
				auto dst_pixel = (image_pixel_color*)dst_pixelbuffer;
				auto src_pixel = (image_pixel_color*)src_pixelbuffer;

				dst_pixel->rgba = 0xFFFFFFFF;
				dst_pixel->a = default_alpha_value;
				for (u8 j = 0; j < image->components; ++j)
					dst_pixel->data[j] = src_pixel->data[j];

				src_pixelbuffer += image->components;
				dst_pixelbuffer += 4;
			}

			return copy_img;
		}

		void image__try_flip_channels(image_t* image)
		{
			auto logger = g_image_state.logger;
			if (image->components != 4) {
				logger->warn(strings::logs::g_image_cant_flip_channels);
				return;
			}

			byte* pixelbuffer;
			u32 size;
			image_get_pixelbuffer(image, &pixelbuffer, &size);

			image_pixel_color tmp_pixel;
			for (u32 i = 0; i < size; i += 4) {
				auto pixel = (image_pixel_color*)pixelbuffer[i];
				tmp_pixel.r = pixel->b;
				tmp_pixel.g = pixel->g;
				tmp_pixel.b = pixel->r;
				tmp_pixel.a = pixel->a;

				*pixel = tmp_pixel;
			}
		}
	}
}
