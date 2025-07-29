#pragma once
#include <rengine/api.h>
#include <rengine/eastl.h>

namespace rengine {
    namespace io {
        enum class file_encoding_type: u8 {
            unknown = 0,
            utf8,
            utf16_be,
            utf16_le,
            utf32_be,
            utf32_le,
            utf7,
            utf1,
            utf_ebcdic,
            scsu,
            bocu1,
            gb18030
        };

        R_EXPORT bool file_exists(c_str path);
        R_EXPORT void file_read(c_str path, size_t* file_size, byte** output_data);
        R_EXPORT string file_read_text(c_str path);
        R_EXPORT file_encoding_type file_get_encoding(c_str path);
    }
}