#include "path.h"

#include <filesystem>

namespace rengine {
    namespace io {
        R_EXPORT void path_normalize(string* str) {
            std::filesystem::path p = std::filesystem::absolute(str->c_str());
            *str = p.string().c_str();
        }

        R_EXPORT string path_combine(c_str base_path, u32 num_sub_paths, c_str* paths) {
            std::filesystem::path p = base_path;
            for (u32 i = 0; i < num_sub_paths; i++) {
                p /= paths[i];
            }

            return p.string().c_str();
        }
    }
}
