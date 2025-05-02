#include "./quaternion.h"
#include "./matrix3x3.h"

namespace rengine {
    namespace math {
		const matrix3x3 quat::to_matrix() const
		{
			return matrix3x3(
				1. - 2. * y * y - 2. * z * z,
				2. * x * y - 2. * w * z,
				2. * x * z + 2. * w * y,
				2. * x * y + 2. * w * z,
				1. - 2. * x * x - 2. * z * z,
				2. * y * z - 2. * w * x,
				2. * x * z - 2. * w * y,
				2. * y * z + 2. * w * x,
				1. - 2. * x * x - 2. * y * y
			);
		}

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

		quat quat::from_rotation(number_t degrees)
		{
			return from_euler_angles(vec3::from_z(degrees));
		}

		quat quat::from_euler_angles(const vec3& angles)
		{
			quat ret;

			ret.x = angles.x * deg_2_rad_ratio;
			ret.y = angles.y * deg_2_rad_ratio;
			ret.z = angles.z * deg_2_rad_ratio;

			number_t sinX = math::sin(ret.x);
			number_t cosX = math::cos(ret.x);
			number_t sinY = math::sin(ret.y);
			number_t cosY = math::cos(ret.y);
			number_t sinZ = math::sin(ret.z);
			number_t cosZ = math::cos(ret.z);

			ret.w = cosY * cosX * cosZ + sinY * sinX * sinZ;
			ret.x = cosY * sinX * cosZ + sinY * cosX * sinZ;
			ret.y = sinY * cosX * cosZ - cosY * sinX * sinZ;
			ret.z = cosY * cosX * sinZ - sinY * sinX * cosZ;
			return ret;
		}
    }
}
