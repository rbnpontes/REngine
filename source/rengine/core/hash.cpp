#include "./hash.h"
#include "./number_utils.h"

#include <utility>
#include <xxHash/xxhash.h>

namespace rengine {
	namespace core {
		hash_t hash(c_str str) {
			if (!str)
				return 0x0;
			return XXH32(str, strlen(str), CORE_DEFAULT_HASH_SEED);
		}

		hash_t hash(const ptr _ptr) {
			if (!_ptr)
				return 0x0;
			return XXH32(_ptr, sizeof(ptr), CORE_DEFAULT_HASH_SEED);
		}

		hash_t hash(u32 value)
		{
			return XXH32(&value, sizeof(u32), CORE_DEFAULT_HASH_SEED);
		}

		hash_t hash(u64 value)
		{
			return XXH32(&value, sizeof(u32) * 2, CORE_DEFAULT_HASH_SEED);
		}

		hash_t hash(float value)
		{
			number_conversor conversor{
				.num_32{.f = value }
			};
			return hash(conversor.num_32.u);
		}

		hash_t hash(double value)
		{
			number_conversor conversor{
				.num_64{.d = value }
			};
			return hash(conversor.num_64.u);
		}

		hash_t hash(const byte* values, u32 count)
		{
			return XXH32(values, sizeof(byte) * count, CORE_DEFAULT_HASH_SEED);
		}

		hash_t hash(const u16* values, u32 count) {
			return XXH32(values, sizeof(u16) * count, CORE_DEFAULT_HASH_SEED);
		}

		hash_t hash(const u32* values, u32 count)
		{
			return XXH32(values, sizeof(u32) * count, CORE_DEFAULT_HASH_SEED);
		}

		hash_t hash_combine(hash_t first, hash_t second) {
			return (first * CORE_HASH_PRIME) ^ second;
		}

		hash_t hash_fast_combine(hash_t first, hash_t second)
		{
			return (first << 16u) | (second >> 16) ^ second;
		}
	}
}