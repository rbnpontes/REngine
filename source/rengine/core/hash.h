#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

namespace rengine {
	namespace core {
		R_EXPORT hash_t hash(c_str str);
		R_EXPORT hash_t hash(ptr _ptr);
		R_EXPORT hash_t hash(u32 value);
		R_EXPORT hash_t hash(byte* values, u32 count);
		R_EXPORT hash_t hash(u16* values, u32 count);
		R_EXPORT hash_t hash(u32* values, u32 count);
		R_EXPORT hash_t hash_combine(hash_t first, hash_t second);
		R_EXPORT hash_t hash_fast_combine(hash_t first, hash_t second);
	}
}