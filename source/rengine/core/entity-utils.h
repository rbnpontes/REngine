#pragma once
#include <rengine/types.h>

namespace rengine {
	namespace core {

		template<typename Entity>
		struct entity_id_encoder {
			static constexpr Entity encode(u32 idx, u32 magic);
			static constexpr u32 decode(Entity value);
		};

		template<>
		struct entity_id_encoder<u8> {
			static constexpr u8 encode(u32 idx, u32 magic) {
				return idx;
			}
			static constexpr u32 decode(u8 value) {
				return value;
			}
		};

		template<>
		struct entity_id_encoder<u16> {
			static constexpr u16 encode(u32 idx, u32 magic) {
				return (u16)((idx << 8) | magic);
			}
			static constexpr u32 decode(u16 value) {
				return (u32)(value >> 8);
			}
		};

		template<>
		struct entity_id_encoder<u32> {
			static constexpr u32 encode(u32 idx, u32 magic) {
				return (idx << 16) | magic;
			}
			static constexpr u32 decode(u32 value) {
				return (u32)(value >> 16);
			}
		};
	}
}