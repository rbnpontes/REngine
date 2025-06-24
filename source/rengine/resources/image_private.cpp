#include "image_private.h"
#include "../core/allocator.h"
#include "../io/logger.h"
#include "../strings.h"
#include <fmt/format.h>

namespace rengine {
        namespace resources {
                void image__init()
                {
                        g_image_state = {};
                        g_image_state.log = io::logger_use(strings::logs::g_image_tag);
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
                                const auto log = g_image_state.log;
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
                        if(state.root)
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
			
			if(prev)
				prev->next = next;
			if(next)
				next->prev = prev;

			if (state.root == image)
				state.root = next;
			if (!state.root)
				state.root = prev;

			state.num_images--;
		}
	}
}
