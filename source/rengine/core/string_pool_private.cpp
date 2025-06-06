#include "string_pool_private.h"
#include "./allocator.h"
#include "./string_pool.h"

namespace rengine {
	namespace core {
		string_pool_state g_string_pool_state = {};
		
		void string_pool__init()
		{
		}

		void string_pool__deinit()
		{
			string_pool_clear();
		}
	}
}