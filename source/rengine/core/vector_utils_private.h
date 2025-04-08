#pragma once
#include "../base_private.h"

namespace rengine {
	namespace core {
		template <typename T>
		void vector_utils_insert_item(vector<T>& vec, const T& item, u32* curr_count) {
			if (*curr_count < vec.size())
				vec[*curr_count] = item;
			else
				vec.push_back(item);
			++(*curr_count);
		}
	}
}