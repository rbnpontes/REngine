#pragma once
#include "../base_private.h"

namespace rengine {
	namespace core {
		struct string_pool_state {
			hash_map<core::hash_t, c_str> strings;
			size_t num_strings;
		};
		extern string_pool_state g_string_pool_state;

		void string_pool__init();
		void string_pool__deinit();
	}
}