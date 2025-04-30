#include "./quaternion.h"
#include "./matrix3x3.h"

namespace rengine {
    namespace math {
        quat quat::from_matrix3x3(const matrix3x3& matrix)
        {
            quat ret;
			number_t inv_s;
            auto t = matrix.m[0][0] + matrix.m[1][1] + matrix.m[2][2];

            if (t > 0.) {
                inv_s = .5 / math::sqrt(1. + t);

				ret.x = (matrix.m[2][1] - matrix.m[1][2]) * inv_s;
				ret.y = (matrix.m[0][2] - matrix.m[2][0]) * inv_s;
				ret.z = (matrix.m[1][0] - matrix.m[0][1]) * inv_s;
				ret.w = .25 / inv_s;

				return ret;
            }

            if (matrix.m[0][0] > matrix.m[1][1] && matrix.m[0][0] > matrix.m[2][2]) {
				inv_s = .5 / math::sqrt(1. + matrix.m[0][0] - matrix.m[1][1] - matrix.m[2][2]);

				ret.x = .25 / inv_s;
				ret.y = (matrix.m[0][1] + matrix.m[1][0]) * inv_s;
				ret.z = (matrix.m[2][0] + matrix.m[0][2]) * inv_s;
				ret.w = (matrix.m[2][1] - matrix.m[1][2]) * inv_s;
                
				return ret;
            }

            if (matrix.m[1][1] > matrix.m[2][2]) {
				inv_s = .5 / math::sqrt(1. + matrix.m[1][1] - matrix.m[0][0] - matrix.m[2][2]);

				ret.x = (matrix.m[0][1] + matrix.m[1][0]) * inv_s;
				ret.y = .25 / inv_s;
				ret.z = (matrix.m[1][2] + matrix.m[2][1]) * inv_s;
				ret.w = (matrix.m[0][2] - matrix.m[2][0]) * inv_s;

				return ret;
            }

			inv_s = .5 / math::sqrt(1. + matrix.m[2][2] - matrix.m[0][0] - matrix.m[1][1]);

			ret.x = (matrix.m[0][2] + matrix.m[2][0]) * inv_s;
			ret.y = (matrix.m[1][2] + matrix.m[2][1]) * inv_s;
			ret.z = .25 / inv_s;
			ret.w = (matrix.m[1][0] - matrix.m[0][1]) * inv_s;
            return ret;
        }
    }
}
