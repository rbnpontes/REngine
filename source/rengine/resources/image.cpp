#include "image.h"
#include "./image_private.h"

namespace rengine {
	namespace resources {
		image_t* image_create(const image_create_desc& desc)
		{
			return image__alloc(desc);
		}
	}
}
