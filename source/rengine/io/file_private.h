#pragma once
#include "../base_private.h"
#include "file.h"

#include  <fstream>

namespace rengine {
    namespace io {
        static constexpr byte g_file_encoding_values[] = {
        // num | encoding values
            3, 0xEF, 0xBB, 0xBF,
            3, 0xFE, 0xFF, 0x01,
            3, 0xFF, 0xFE, 0x01,
            4, 0x00, 0x00, 0xFE, 0xFF,
            4, 0xFF, 0xFE, 0x00, 0x00,
            3, 0x2B, 0x2F, 0x76,
            3, 0xF7, 0x64, 0x4C,
            4, 0xDD, 0x73, 0x66, 0x73,
            3, 0x0E, 0xFE, 0xFF,
            3, 0xFB, 0xEE, 0x28,
            4, 0x84, 0x31, 0x95, 0x33,
            0
        };

        void file__try_open(c_str path, std::ifstream* output);
        file_encoding_type file__get_encoding(std::ifstream& input, u8* offset);
    }
}