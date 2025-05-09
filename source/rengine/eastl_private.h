#pragma once
#define EASTL_ALLOCATOR_DEFAULT_NAME "REngine"

#include <EASTL/shared_ptr.h>
#include <EASTL/string.h>
#include <EASTL/string_view.h>
#include <EASTL/array.h>
#include <EASTL/vector.h>
#include <EASTL/fixed_vector.h>
#include <EASTL/queue.h>
#include <EASTL/stack.h>
#include <EASTL/map.h>
#include <EASTL/hash_map.h>
#include <EASTL/unordered_map.h>
#include <EASTL/internal/integer_sequence.h>

namespace rengine {
	template<typename T, size_t N = 1>
	using array = eastl::array<T, N>;
	using string = eastl::string;
	using string_view = eastl::string_view;
	template<typename T>
	using vector = eastl::vector<T>;
	template<typename T, size_t N = 1>
	using fixed_vector = eastl::fixed_vector<T, N>;
	template<typename T>
	using queue = eastl::queue<T>;
	template<typename T>
	using stack = eastl::stack<T>;
	template<typename Key, typename Value>
	using unordered_map = eastl::unordered_map<Key, Value>;
	template<typename Key, typename Value>
	using map = eastl::map<Key, Value>;
	template<typename Key, typename Value>
	using hash_map = eastl::hash_map<Key, Value>;
	template<typename T>
	using shared_ptr = eastl::shared_ptr<T>;
}