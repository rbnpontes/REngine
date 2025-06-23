#pragma once
#include "./image.h"
#include "../io/logger.h"
#include "../strings.h"

namespace rengine {
	namespace resources {
		struct image_t {
			image_t* prev{ null };
			image_t* next{ null };
			math::uvec2 size{ 1, 1 };
			u8 components{ 4 };
		};

               struct image_state {
                       u32 num_images{ 0 };
                       image_t* root{ null };
                        io::ILog* log{ null };
               };

               void image__validate_pos(const image_t* img, math::uvec2& pos);

		extern image_state g_image_state;

		void image__init();
		void image__deinit();
		image_t* image__alloc(const image_create_desc& desc);
		void image__free_images();
		void image__destroy(image_t* image);
	}
}