#pragma once
#include <rengine/api.h>
#include <rengine/eastl.h>

namespace rengine {
    namespace io {
        R_EXPORT void path_normalize(string* str);
        R_EXPORT string path_combine_ex(c_str base_path, u32 num_sub_paths, c_str* paths);

        template<typename... Args>
        inline vector<string> path_combine(c_str base_path, Args... paths) {
            static_assert((std::is_same_v<Args, c_str> && ...), "Must be c_str type");

            constexpr u32 count = sizeof...(paths);
            c_str arr[] = { paths... };
            return path_combine_ex(base_path, count, arr);
        }
    }
}