#pragma once
#include <rengine/math/math-types.h>
#include <rengine/math/math-operations.h>

namespace rengine {
	namespace math {
        typedef struct matrix3x3;

        struct R_EXPORT quat {
            number_t w, x, y, z;

            constexpr quat() : x(0.), y(0.), z(0.), w(1.) {}
            quat(const number_t* data) {
                sse_store_number(&w, sse_load_number(data));
            }

            static quat from_matrix3x3(const matrix3x3& matrix);
        };
	}
}