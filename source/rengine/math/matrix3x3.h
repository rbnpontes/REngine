#pragma once
#include <rengine/types.h>
#include <rengine/math/math-types.h>

namespace rengine {
	namespace math {
		struct matrix3x3 {
			number_t m[3][3];

			constexpr matrix3x3() {
				for (int i = 0; i < 3; ++i) {
					for (int j = 0; j < 3; ++j) {
						m[i][j] = 0;
					}
				}
			}
			constexpr matrix3x3(number_t v00, number_t v01, number_t v02,
				number_t v10, number_t v11, number_t v12,
				number_t v20, number_t v21, number_t v22) {
				m[0][0] = v00; m[0][1] = v01; m[0][2] = v02;
				m[1][0] = v10; m[1][1] = v11; m[1][2] = v12;
				m[2][0] = v20; m[2][1] = v21; m[2][2] = v22;
			}

			constexpr matrix3x3(const number_t* data) {
				for (int i = 0; i < 3; ++i) {
					for (int j = 0; j < 3; ++j) {
						m[i][j] = data[i * 3 + j];
					}
				}
			}

			bool equals(const matrix3x3& rhs) const;
			vec3 mul(const vec3& rhs) const;
			matrix3x3 mul(number_t rhs) const;
			matrix3x3 mul(const matrix3x3& rhs) const;
			matrix3x3 add(const matrix3x3& rhs) const;
			matrix3x3 sub(const matrix3x3& rhs) const;
			matrix3x3 scaled(const vec3& scale) const;
			vec3 scale() const;
			vec3 signed_scale(const matrix3x3& rotation) const;
			matrix3x3 transpose() const;
			matrix3x3 inverse() const;

			void set_scale(const vec3& scale);
			void set_scale(number_t scale);
		};
	}
}