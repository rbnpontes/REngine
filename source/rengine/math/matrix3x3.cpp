#include "./matrix3x3.h"

namespace rengine {
	namespace math {
		bool matrix3x3::equals(const matrix3x3& rhs) const
		{
			for (int i = 0; i < 3; ++i) {
				for (int j = 0; j < 3; ++j) {
					if (math::equals(m[i][j], rhs.m[i][j]))
						continue;
					return false;
				}
			}
			return true;
		}

		vec3 matrix3x3::mul(const vec3& rhs) const
		{
			return vec3(
				m[0][0] * rhs.x + m[0][1] * rhs.y + m[0][2] * rhs.z,
				m[1][0] * rhs.x + m[1][1] * rhs.y + m[1][2] * rhs.z,
				m[2][0] * rhs.x + m[2][1] * rhs.y + m[2][2] * rhs.z
			);
		}

		matrix3x3 matrix3x3::mul(number_t rhs) const
		{
			return matrix3x3(
				m[0][0] * rhs,
				m[0][1] * rhs,
				m[0][2] * rhs,
				m[1][0] * rhs,
				m[1][1] * rhs,
				m[1][2] * rhs,
				m[2][0] * rhs,
				m[2][1] * rhs,
				m[2][2] * rhs
			);
		}

		matrix3x3 matrix3x3::mul(const matrix3x3& rhs) const
		{
			return matrix3x3(
				m[0][0] * rhs.m[0][0] + m[0][1] * rhs.m[1][0] + m[0][2] * rhs.m[2][0],
				m[0][0] * rhs.m[0][1] + m[0][1] * rhs.m[1][1] + m[0][2] * rhs.m[2][1],
				m[0][0] * rhs.m[0][2] + m[0][1] * rhs.m[1][2] + m[0][2] * rhs.m[2][2],
				m[1][0] * rhs.m[0][0] + m[1][1] * rhs.m[1][0] + m[1][2] * rhs.m[2][0],
				m[1][0] * rhs.m[0][1] + m[1][1] * rhs.m[1][1] + m[1][2] * rhs.m[2][1],
				m[1][0] * rhs.m[0][2] + m[1][1] * rhs.m[1][2] + m[1][2] * rhs.m[2][2],
				m[2][0] * rhs.m[0][0] + m[2][1] * rhs.m[1][0] + m[2][2] * rhs.m[2][0],
				m[2][0] * rhs.m[0][1] + m[2][1] * rhs.m[1][1] + m[2][2] * rhs.m[2][1],
				m[2][0] * rhs.m[0][2] + m[2][1] * rhs.m[1][2] + m[2][2] * rhs.m[2][2]
			);
		}

		matrix3x3 matrix3x3::add(const matrix3x3& rhs) const
		{
			return matrix3x3(
				m[0][0] + rhs.m[0][0],
				m[0][1] + rhs.m[0][1],
				m[0][2] + rhs.m[0][2],
				m[1][0] + rhs.m[1][0],
				m[1][1] + rhs.m[1][1],
				m[1][2] + rhs.m[1][2],
				m[2][0] + rhs.m[2][0],
				m[2][1] + rhs.m[2][1],
				m[2][2] + rhs.m[2][2]
			);
		}

		matrix3x3 matrix3x3::sub(const matrix3x3& rhs) const
		{
			return matrix3x3(
				m[0][0] - rhs.m[0][0],
				m[0][1] - rhs.m[0][1],
				m[0][2] - rhs.m[0][2],
				m[1][0] - rhs.m[1][0],
				m[1][1] - rhs.m[1][1],
				m[1][2] - rhs.m[1][2],
				m[2][0] - rhs.m[2][0],
				m[2][1] - rhs.m[2][1],
				m[2][2] - rhs.m[2][2]
			);
		}

		matrix3x3 matrix3x3::scaled(const vec3& scale) const
		{
			return matrix3x3(
				m[0][0] * scale.x,
				m[0][1] * scale.y,
				m[0][2] * scale.z,
				m[1][0] * scale.x,
				m[1][1] * scale.y,
				m[1][2] * scale.z,
				m[2][0] * scale.x,
				m[2][1] * scale.y,
				m[2][2] * scale.z
			);
		}

		vec3 matrix3x3::scale() const
		{
			return vec3(
				math::sqrt(m[0][0] * m[0][0] + m[1][0] * m[1][0] + m[2][0] * m[2][0]),
				math::sqrt(m[0][1] * m[0][1] + m[1][1] * m[1][1] + m[2][1] * m[2][1]),
				math::sqrt(m[0][2] * m[0][2] + m[1][2] * m[1][2] + m[2][2] * m[2][2])
			);
		}

		vec3 matrix3x3::signed_scale(const matrix3x3& rotation) const
		{
			return vec3(
				rotation.m[0][0] * m[0][0] + rotation.m[1][0] * m[1][0] + rotation.m[2][0] * m[2][0],
				rotation.m[0][1] * m[0][1] + rotation.m[1][1] * m[1][1] + rotation.m[2][1] * m[2][1],
				rotation.m[0][2] * m[0][2] + rotation.m[1][2] * m[1][2] + rotation.m[2][2] * m[2][2]
			);
		}

		matrix3x3 matrix3x3::transpose() const
		{
			return matrix3x3(
				m[0][0], 
				m[1][0],
				m[2][0],
				m[0][1],
				m[1][1],
				m[2][1],
				m[0][2],
				m[1][2],
				m[2][2]
			);
		}

		matrix3x3 matrix3x3::inverse() const
		{
			number_t det = m[0][0] * m[1][1] * m[2][2] +
							m[1][0] * m[2][1] * m[0][2] +
							m[2][0] * m[0][1] * m[1][2] -
							m[2][0] * m[1][1] * m[0][2] -
							m[1][0] * m[0][1] * m[2][2] -
							m[0][0] * m[2][1] * m[1][2];
			number_t inv_det = 1. / det;
			return matrix3x3(
				(m[1][1] * m[2][2] - m[2][1] * m[1][2]) * inv_det,
				-(m[0][1] * m[2][2] - m[2][1] * m[0][2]) * inv_det,
				(m[0][1] * m[1][2] - m[1][1] * m[0][2]) * inv_det,
				-(m[1][0] * m[2][2] - m[2][0] * m[1][2]) * inv_det,
				(m[0][0] * m[2][2] - m[2][0] * m[0][2]) * inv_det,
				-(m[0][0] * m[1][2] - m[1][0] * m[0][2]) * inv_det,
				(m[1][0] * m[2][1] - m[2][0] * m[1][1]) * inv_det,
				-(m[0][0] * m[2][1] - m[2][0] * m[0][1]) * inv_det,
				(m[0][0] * m[1][1] - m[1][0] * m[0][1]) * inv_det
			);
		}

		void matrix3x3::set_scale(const vec3& scale)
		{
			m[0][0] = scale.x;
			m[1][1] = scale.y;
			m[2][2] = scale.z;
		}

		void matrix3x3::set_scale(number_t scale)
		{
			m[0][0] = m[1][1] = m[2][2] = scale;
		}
	}
}