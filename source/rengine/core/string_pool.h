#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

namespace rengine {
    namespace core {
        R_EXPORT c_str string_pool_intern(const c_str str, core::hash_t* output_hash = null);
        R_EXPORT c_str string_pool_get_from_hash(core::hash_t hash);
		R_EXPORT void string_pool_uncache(core::hash_t hash);
        R_EXPORT void string_pool_clear();
        R_EXPORT size_t string_pool_num_stored_strings();
    }
}