#include "image_private.h"
#include "../core/allocator.h"

namespace rengine {
	namespace resources {
		void image__init()
		{
			g_image_state = {};
		}
		
		void image__deinit()
		{
			image__free_images();
			g_image_state = {};
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
