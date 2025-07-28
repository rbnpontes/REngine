#pragma once
#include <rengine/api.h>
#include <rengine/eastl.h>

namespace rengine {
    namespace io {
        R_EXPORT bool file_exists(c_str path);
        R_EXPORT void file_read(c_str path, size_t* file_size, byte** output_data);

        inline string file_read_text(c_str path) {
            size_t file_size;
            string output_data;

            file_read(path, &file_size, null);

            if (0 == file_size)
                return output_data;

            output_data.resize(file_size);

            byte* write_data = (byte*)output_data.data();
            file_read(path, &file_size, &write_data);
            return output_data;
        }
    }
}