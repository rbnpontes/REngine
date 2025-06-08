#include "./string_pool.h"
#include "./hash.h"
#include "./allocator.h"
#include "./string_pool_private.h"

namespace rengine {
    namespace core {
        c_str rengine::core::string_pool_intern(const c_str str, core::hash_t* output_hash)
        {
			auto& state = g_string_pool_state;
            auto str_hash = hash(str);
			auto pair = state.strings.find_as(str_hash);

            if (output_hash)
                *output_hash = str_hash;
			if (state.strings.end() != pair)
				return pair->second;

			char* intern_str = (char*)alloc(strlen(str) + 1);
            strcpy(intern_str, str);

			state.strings[str_hash] = intern_str;
            ++state.num_strings;

            return intern_str;
        }

        c_str string_pool_get_from_hash(core::hash_t hash)
        {
            auto& state = g_string_pool_state;
            auto pair = state.strings.find_as(hash);

            if (state.strings.end() == pair)
                return null;

            return pair->second;
        }

        void string_pool_uncache(core::hash_t hash)
        {
            auto& state = g_string_pool_state;
            auto pair = state.strings.find_as(hash);

            if (state.strings.end() == pair)
                return;

            core::alloc_free((ptr)pair->second);
			state.strings.erase(pair);
            --state.num_strings;
        }
        
        void string_pool_clear()
        {
            auto& state = g_string_pool_state;
            for (auto& pair : state.strings) {
                auto str = (ptr)pair.second;
                core::alloc_free(str);
            }
            state.strings.clear();
            state.num_strings = 0;
        }
        
        size_t string_pool_num_stored_strings()
        {
			return g_string_pool_state.num_strings;
        }
    }
}
