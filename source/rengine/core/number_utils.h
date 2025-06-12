#pragma once
#include <rengine/types.h>

namespace rengine {
	namespace core {
		struct number_conversor {
			union {
				float f;
				u32 u;
				i32 i;
			} num_32;
			union {
				double d;
				u64 u;
				i64 i;
			} num_64;
		};
	}
}