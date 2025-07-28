#include "file.h"
#include "../exceptions.h"
#include "../strings.h"

#include <filesystem>
#include <fstream>


namespace rengine {
    namespace io {
        bool file_exists(c_str path) {
            return std::filesystem::exists(path);
        }

        void file_read(c_str path, size_t *file_size, byte **output_data) {
            if (!file_exists(path))
                throw io::file_exception(strings::exceptions::g_file_not_found);

            std::ifstream in{ path, std::ios::binary | std::ios::ate };
            if (!in.is_open())
                throw io::file_exception(strings::exceptions::g_file_failed_2_open);

            const auto size = in.tellg();
            in.seekg(0, std::ios::beg);

            *file_size = size;

            if (!output_data)
                return;

            if (in.read(reinterpret_cast<char*>(*output_data), size))
                return;

            throw io::file_exception(strings::exceptions::g_file_failed_2_read);
        }

    }
}
