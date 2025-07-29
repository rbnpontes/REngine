#pragma once
#include <rengine/types.h>

namespace rengine {
	namespace core {
		union u32_packer {
			u8 b[4];
			i8 sb[4];
			float f;
			u32 u;
			i32 i;
		};

		union u64_packer {
			u8 b[8];
			i8 sb[8];
			double d;
			u64 u;
			i64 i;
		};

		struct number_conversor {
			u32_packer num_32;
			u64_packer num_64;
		};
	}
}