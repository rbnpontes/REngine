#include "file.h"
#include "file_private.h"

#include "../exceptions.h"
#include "../strings.h"

#include <filesystem>
#include <fstream>

#include "fmt/base.h"


namespace rengine {
    namespace io {
        bool file_exists(c_str path) {
            return std::filesystem::exists(path);
        }

        void file_read(c_str path, size_t *file_size, byte **output_data) {
            std::ifstream input;
            file__try_open(path, &input);

            const auto size = input.tellg();
            input.seekg(0, std::ios::beg);

            *file_size = size;

            if (!output_data)
                return;

            if (input.read(reinterpret_cast<char*>(*output_data), size))
                return;

            throw io::file_exception(strings::exceptions::g_file_failed_2_read);
        }

        string file_read_text(c_str path) {
            string result;
            std::ifstream input;
            file__try_open(path, &input);

            size_t size = input.tellg();

            u8 encoding_offset = 0;
            file__get_encoding(input, &encoding_offset);

            size -= encoding_offset;
            input.seekg(encoding_offset, std::ios::beg);

            result.resize(size);

            if (input.read(reinterpret_cast<char*>(result.data()), size))
                return result;

            throw io::file_exception(strings::exceptions::g_file_failed_2_read);
        }

        file_encoding_type file_get_encoding(c_str path) {
            std::ifstream input;
            u8 encoding_offset = 0;

            file__try_open(path, &input);
            return file__get_encoding(input, &encoding_offset);
        }
    }
}
