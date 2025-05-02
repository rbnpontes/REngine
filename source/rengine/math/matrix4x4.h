#pragma once
#include <rengine/types.h>
#include <rengine/math/math-types.h>
#include <rengine/math/vec3.h>

namespace rengine {
	namespace math {
		struct matrix3x3;
		struct quat;

		struct R_EXPORT matrix4x4 {
			number_t m[4][4];
			constexpr matrix4x4() {
				for (int i = 0; i < 4; ++i) {
					for (int j = 0; j < 4; ++j) {
						m[i][j] = 0;
					}
				}
			}

			constexpr matrix4x4(number_t m00, number_t m01, number_t m02, number_t m03,
				number_t m10, number_t m11, number_t m12, number_t m13,
				number_t m20, number_t m21, number_t m22, number_t m23,
				number_t m30, number_t m31, number_t m32, number_t m33)
			{
				m[0][0] = m00; m[0][1] = m01; m[0][2] = m02; m[0][3] = m03;
				m[1][0] = m10; m[1][1] = m11; m[1][2] = m12; m[1][3] = m13;
				m[2][0] = m20; m[2][1] = m21; m[2][2] = m22; m[2][3] = m23;
				m[3][0] = m30; m[3][1] = m31; m[3][2] = m32; m[3][3] = m33;
			}
			
			matrix4x4(const number_t* data) {
				// reference: https://github.com/u3d-community/U3D/blob/383b74222188a8d301aaf93061e26c9d8efdc825/Source/Urho3D/Math/Matrix4.h#L176
				sse_store_number(&m[0][0], sse_load_number(data));
				sse_store_number(&m[1][0], sse_load_number(data + 4));
				sse_store_number(&m[2][0], sse_load_number(data + 8));
				sse_store_number(&m[3][0], sse_load_number(data + 12));
			}

			bool equals(const matrix4x4& rhs) const;

			static vec3 mul(const matrix4x4& m, const vec3& rhs);

			static vec4 mul(const matrix4x4& m, const vec4& rhs);

			static matrix4x4 mul(const matrix4x4& m, number_t rhs);

			static matrix4x4 mul(const matrix4x4& m, const matrix4x4& rhs);

			matrix4x4 add(const matrix4x4& rhs) const;

			matrix4x4 sub(const matrix4x4& rhs) const;

			void set_translation(const vec3& translation);

			void set_rotation(const matrix3x3& rotation);

			void set_scale(const vec3& scale);

			void set_scale(number_t scale);

			matrix3x3 to_matrix3x3() const;

			matrix3x3 rotation_matrix() const;

			vec3 translation() const;

			quat rotation() const;

			vec3 scale() const;

			vec3 signed_scale(const matrix3x3& rotation) const;

			matrix4x4 transpose() const;

			void decompose(vec3& translation, quat& rotation, vec3& scale) const;

			static matrix4x4 from_translation(const vec3& value);
			static matrix4x4 from_rotation(const quat& value);
			static matrix4x4 from_rotation(const matrix3x3& value);
			static matrix4x4 from_scale(number_t value);
			static matrix4x4 from_scale(const vec3& value);

			static matrix4x4 inverse(const matrix4x4& matrix);

			static matrix4x4 transform(const vec3& translation, const quat& rotation, const vec3& scale);

			static const matrix4x4 zero;
			static const matrix4x4 identity;
		};
	}
}