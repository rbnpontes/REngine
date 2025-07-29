#include "file_private.h"

#include "../core/number_utils.h"
#include "../math/math.h"

namespace rengine {
    namespace io {
        void file__try_open(c_str path, std::ifstream* output) {
            string result;
            if (!file_exists(path))
                throw io::file_exception(strings::exceptions::g_file_not_found);

            *output = std::ifstream { path, std::ios::binary | std::ios::ate };
            if (!output->is_open())
                throw io::file_exception(strings::exceptions::g_file_failed_2_open);
        }

        file_encoding_type file__get_encoding(std::ifstream& input, u8* offset) {
            file_encoding_type result = file_encoding_type::unknown;
            const size_t file_size = input.tellg();
            if (file_size < 3)
                return result;

            core::u32_packer bom_header;
            bom_header.u = 0;

            input.seekg(0, std::ios::beg);

            size_t bom_header_size = math::min(file_size, (size_t)3);
            if (!input.read(reinterpret_cast<char*>(&bom_header), bom_header_size))
                throw io::file_exception(strings::exceptions::g_file_failed_2_read);

            *offset = bom_header_size;

            auto val = g_file_encoding_values;

            bool match = false;
            u8 encoding_val_count = 0;
            core::u32_packer encoding_val;
            encoding_val.u = 0;
            while (0 != val) {
                result = (file_encoding_type)((u8)result + 1);
                encoding_val_count = *val;
                encoding_val.u = 0;
                memcpy(&encoding_val, val + 1, encoding_val_count);

                // match encoding
                if (encoding_val.u == bom_header.u) {
                    match = true;
                    break;
                }

                val += encoding_val_count + 1;
            }

            if (!match)
                result = file_encoding_type::unknown;

            input.seekg(file_size, std::ios::beg);
            return result;
        }
    }
}