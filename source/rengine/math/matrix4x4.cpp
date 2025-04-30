#include "./matrix4x4.h"
#include "./matrix3x3.h"
#include "./quaternion.h"

#include "../exceptions.h"

namespace rengine {
	namespace math {
		bool matrix4x4::equals(const matrix4x4& rhs) const {
			// reference: https://github.com/u3d-community/U3D/blob/383b74222188a8d301aaf93061e26c9d8efdc825/Source/Urho3D/Math/Matrix4.h#L227
			auto c0 = sse_cmpeq_number(sse_load_number(&m[0][0]), sse_load_number(&rhs.m[0][0]));
			auto c1 = sse_cmpeq_number(sse_load_number(&m[1][0]), sse_load_number(&rhs.m[1][0]));
			auto c2 = sse_cmpeq_number(sse_load_number(&m[2][0]), sse_load_number(&rhs.m[2][0]));
			auto c3 = sse_cmpeq_number(sse_load_number(&m[3][0]), sse_load_number(&rhs.m[3][0]));

			c0 = sse_and_number(c0, c1);
			c2 = sse_and_number(c2, c3);
			c0 = sse_and_number(c0, c2);

			auto hi = sse_movehl_number(c0, c0);
			c0 = sse_and_number(c0, hi);
			hi = sse_shuffle_number(c0, c0, sse_shuffle(1, 1, 1, 1));
			c0 = sse_and_number(c0, hi);

			return sse_cvtsi128_int(sse_cast_int_number(c0)) == -1;
		}

		vec3 matrix4x4::mul(const vec3& rhs) const {
			// reference: https://github.com/u3d-community/U3D/blob/383b74222188a8d301aaf93061e26c9d8efdc825/Source/Urho3D/Math/Matrix4.h#L258
			auto vec = sse_set_number(1.f, rhs.z, rhs.y, rhs.x);

			auto r0 = sse_mul_number(sse_load_number(&m[0][0]), vec);
			auto r1 = sse_mul_number(sse_load_number(&m[1][0]), vec);
			auto r2 = sse_mul_number(sse_load_number(&m[2][0]), vec);
			auto r3 = sse_mul_number(sse_load_number(&m[3][0]), vec);

			auto t0 = sse_unpacklo_number(r0, r1);
			auto t1 = sse_unpackhi_number(r0, r1);
			auto t2 = sse_unpacklo_number(r2, r3);
			auto t3 = sse_unpackhi_number(r2, r3);

			t0 = sse_add_number(t0, t1);
			t2 = sse_add_number(t2, t3);

			vec = sse_add_number(sse_movelh_number(t0, t2), sse_movehl_number(t2, t0));
			vec = sse_div_number(vec, sse_shuffle_number(vec, vec, sse_shuffle(3, 3, 3, 3)));
			return vec3(
				sse_cvtss_number(vec),
				sse_cvtss_number(sse_shuffle_number(vec, vec, sse_shuffle(1, 1, 1, 1))),
				sse_cvtss_number(sse_movehl_number(vec, vec))
			);
		}

		vec4 matrix4x4::mul(const vec4& rhs) const {
			// reference: https://github.com/u3d-community/U3D/blob/383b74222188a8d301aaf93061e26c9d8efdc825/Source/Urho3D/Math/Matrix4.h#L293
			auto vec = sse_load_number(&rhs.x);
			auto r0 = sse_mul_number(sse_load_number(&m[0][0]), vec);
			auto r1 = sse_mul_number(sse_load_number(&m[1][0]), vec);
			auto r2 = sse_mul_number(sse_load_number(&m[2][0]), vec);
			auto r3 = sse_mul_number(sse_load_number(&m[3][0]), vec);

			auto t0 = sse_unpacklo_number(r0, r1);
			auto t1 = sse_unpackhi_number(r0, r1);
			auto t2 = sse_unpacklo_number(r2, r3);
			auto t3 = sse_unpackhi_number(r2, r3);

			t0 = sse_add_number(t0, t1);
			t2 = sse_add_number(t2, t3);

			vec = sse_add_number(sse_movelh_number(t0, t2), sse_movehl_number(t2, t0));

			vec4 ret;
			sse_store_number(&ret.x, vec);
			return ret;
		}

		matrix4x4 matrix4x4::mul(number_t rhs) const {
			// reference: https://github.com/u3d-community/U3D/blob/383b74222188a8d301aaf93061e26c9d8efdc825/Source/Urho3D/Math/Matrix4.h#L384
			matrix4x4 ret;
			const auto x = sse_set_single_number(rhs);
			sse_store_number(&ret.m[0][0], sse_mul_number(sse_load_number(&m[0][0]), x));
			sse_store_number(&ret.m[1][0], sse_mul_number(sse_load_number(&m[1][0]), x));
			sse_store_number(&ret.m[2][0], sse_mul_number(sse_load_number(&m[2][0]), x));
			sse_store_number(&ret.m[3][0], sse_mul_number(sse_load_number(&m[3][0]), x));
			return ret;
		}

		matrix4x4 matrix4x4::mul(const matrix4x4& rhs) const {
			// reference: https://github.com/u3d-community/U3D/blob/383b74222188a8d301aaf93061e26c9d8efdc825/Source/Urho3D/Math/Matrix4.h#L417
			matrix4x4 ret;

			auto r0 = sse_load_number(&m[0][0]);
			auto r1 = sse_load_number(&m[1][0]);
			auto r2 = sse_load_number(&m[2][0]);
			auto r3 = sse_load_number(&m[3][0]);

			sse_m128_t l, t0, t1, t2, t3;

			for (u8 i = 0; i < 4; ++i) {
				l = sse_load_number(&rhs.m[i][0]);
				t0 = sse_mul_number(sse_shuffle_number(l, l, sse_shuffle(0, 0, 0, 0)), r0);
				t1 = sse_mul_number(sse_shuffle_number(l, l, sse_shuffle(1, 1, 1, 1)), r1);
				t2 = sse_mul_number(sse_shuffle_number(l, l, sse_shuffle(2, 2, 2, 2)), r2);
				t3 = sse_mul_number(sse_shuffle_number(l, l, sse_shuffle(3, 3, 3, 3)), r3);
				sse_store_number(&ret.m[i][0], sse_add_number(sse_add_number(t0, t1), sse_add_number(t2, t3)));
			}

			return ret;
		}
	
		matrix4x4 matrix4x4::add(const matrix4x4& rhs) const {
			// reference: https://github.com/u3d-community/U3D/blob/383b74222188a8d301aaf93061e26c9d8efdc825/Source/Urho3D/Math/Matrix4.h#L320
			matrix4x4 ret;
			sse_store_number(&ret.m[0][0], sse_add_number(sse_load_number(&m[0][0]), sse_load_number(&rhs.m[0][0])));
			sse_store_number(&ret.m[1][0], sse_add_number(sse_load_number(&m[1][0]), sse_load_number(&rhs.m[1][0])));
			sse_store_number(&ret.m[2][0], sse_add_number(sse_load_number(&m[2][0]), sse_load_number(&rhs.m[2][0])));
			sse_store_number(&ret.m[3][0], sse_add_number(sse_load_number(&m[3][0]), sse_load_number(&rhs.m[3][0])));

			return ret;
		}

		matrix4x4 matrix4x4::sub(const matrix4x4& rhs) const {
			// reference: https://github.com/u3d-community/U3D/blob/383b74222188a8d301aaf93061e26c9d8efdc825/Source/Urho3D/Math/Matrix4.h#L352
			matrix4x4 ret;
			sse_store_number(&ret.m[0][0], sse_sub_number(sse_load_number(&m[0][0]), sse_load_number(&rhs.m[0][0])));
			sse_store_number(&ret.m[1][0], sse_sub_number(sse_load_number(&m[1][0]), sse_load_number(&rhs.m[1][0])));
			sse_store_number(&ret.m[2][0], sse_sub_number(sse_load_number(&m[2][0]), sse_load_number(&rhs.m[2][0])));
			sse_store_number(&ret.m[3][0], sse_sub_number(sse_load_number(&m[3][0]), sse_load_number(&rhs.m[3][0])));

			return ret;
		}
	
		void matrix4x4::set_translation(const vec3& translation) {
			m[0][3] = translation.x;
			m[1][3] = translation.y;
			m[2][3] = translation.z;
		}

		void matrix4x4::set_rotation(const matrix3x3& rotation) {
			m[0][0] = rotation.m[0][0];
			m[0][1] = rotation.m[0][1];
			m[0][2] = rotation.m[0][2];
			m[1][0] = rotation.m[1][0];
			m[1][1] = rotation.m[1][1];
			m[1][2] = rotation.m[1][2];
			m[2][0] = rotation.m[2][0];
			m[2][1] = rotation.m[2][1];
			m[2][2] = rotation.m[2][2];
		}

		void matrix4x4::set_scale(const vec3& scale) {
			m[0][0] = scale.x;
			m[1][1] = scale.y;
			m[2][2] = scale.z;
		}

		void matrix4x4::set_scale(number_t scale)
		{
			m[0][0] = m[1][1] = m[2][2] = scale;
		}

		matrix3x3 matrix4x4::to_matrix3x3() const
		{
			matrix3x3 ret;
			for (u8 i = 0; i < 3; ++i) {
				for (u8 j = 0; j < 3; ++j) {
					ret.m[i][j] = m[i][j];
				}
			}
			return ret;
		}

		matrix3x3 matrix4x4::rotation_matrix() const
		{
			vec3 inv_scale = {
				1.f / math::sqrt(m[0][0] * m[0][0] + m[1][0] * m[1][0] + m[2][0] * m[2][0]),
				1.f / math::sqrt(m[0][1] * m[0][1] + m[1][1] * m[1][1] + m[2][1] * m[2][1]),
				1.f / math::sqrt(m[0][2] * m[0][2] + m[1][2] * m[1][2] + m[2][2] * m[2][2])
			};
			return to_matrix3x3().scaled(inv_scale);
		}

		vec3 matrix4x4::translation() const
		{
			return vec3(m[0][3], m[1][3], m[2][3]);
		}

		quat matrix4x4::rotation() const
		{
			throw not_implemented_exception();
		}

		vec3 matrix4x4::scale() const
		{
			return vec3(
				math::sqrt(m[0][0] * m[0][0] + m[1][0] * m[1][0] + m[2][0] * m[2][0]),
				math::sqrt(m[0][1] * m[0][1] + m[1][1] * m[1][1] + m[2][1] * m[2][1]),
				math::sqrt(m[0][2] * m[0][2] + m[1][2] * m[1][2] + m[2][2] * m[2][2])
			);
		}

		vec3 matrix4x4::signed_scale(const matrix3x3& rotation) const
		{
			return vec3(
				rotation.m[0][0] * m[0][0] + rotation.m[1][0] * m[1][0] + rotation.m[2][0] * m[2][0],
				rotation.m[0][1] * m[0][1] + rotation.m[1][1] * m[1][1] + rotation.m[2][1] * m[2][1],
				rotation.m[0][2] * m[0][2] + rotation.m[1][2] * m[1][2] + rotation.m[2][2] * m[2][2]
			);
		}

		matrix4x4 matrix4x4::transpose() const
		{
			auto m0 = sse_load_number(&m[0][0]);
			auto m1 = sse_load_number(&m[1][0]);
			auto m2 = sse_load_number(&m[2][0]);
			auto m3 = sse_load_number(&m[3][0]);
			sse_transpose_number(m0, m1, m2, m3);

			matrix4x4 ret;
			sse_store_number(&ret.m[0][0], m0);
			sse_store_number(&ret.m[1][0], m1);
			sse_store_number(&ret.m[2][0], m2);
			sse_store_number(&ret.m[3][0], m3);
			return ret;
		}

		void matrix4x4::decompose(vec3& translation, quat& rotation, vec3& scale) const
		{
			translation.x = m[0][3];
			translation.y = m[1][3];
			translation.z = m[2][3];

			scale.x = math::sqrt(m[0][0] * m[0][0] + m[1][0] * m[1][0] + m[2][0] * m[2][0]);
			scale.y = math::sqrt(m[0][1] * m[0][1] + m[1][1] * m[1][1] + m[2][1] * m[2][1]);
			scale.z = math::sqrt(m[0][2] * m[0][2] + m[1][2] * m[1][2] + m[2][2] * m[2][2]);

			vec3 inv_scale(1. / scale.x, 1. / scale.y, 1. / scale.z);
			rotation = quat::from_matrix3x3(to_matrix3x3().scaled(inv_scale));
		}

		matrix4x4 matrix4x4::inverse(const matrix4x4& matrix)
		{
			number_t v0 = matrix.m[2][0] * matrix.m[3][1] - matrix.m[2][1] * matrix.m[3][0];
			number_t v1 = matrix.m[2][0] * matrix.m[3][2] - matrix.m[2][2] * matrix.m[3][0];
			number_t v2 = matrix.m[2][0] * matrix.m[3][3] - matrix.m[2][3] * matrix.m[3][0];
			number_t v3 = matrix.m[2][1] * matrix.m[3][2] - matrix.m[2][2] * matrix.m[3][1];
			number_t v4 = matrix.m[2][1] * matrix.m[3][3] - matrix.m[2][3] * matrix.m[3][1];
			number_t v5 = matrix.m[2][2] * matrix.m[3][3] - matrix.m[2][3] * matrix.m[3][2];

			number_t i00 = (v5 * matrix.m[1][1] - v4 * matrix.m[1][2] + v3 * matrix.m[1][3]);
			number_t i10 = -(v5 * matrix.m[1][0] - v2 * matrix.m[1][2] + v1 * matrix.m[1][3]);
			number_t i20 = (v4 * matrix.m[1][0] - v2 * matrix.m[1][1] + v0 * matrix.m[1][3]);
			number_t i30 = -(v3 * matrix.m[1][0] - v1 * matrix.m[1][1] + v0 * matrix.m[1][2]);

			number_t inv_det = 1. / (i00 * matrix.m[0][0] + i10 * matrix.m[0][1] + i20 * matrix.m[0][2] + i30 * matrix.m[0][3]);

			i00 *= inv_det;
			i10 *= inv_det;
			i20 *= inv_det;
			i30 *= inv_det;

			number_t i01 = -(v5 * matrix.m[0][1] - v4 * matrix.m[0][2] + v3 * matrix.m[0][3]) * inv_det;
			number_t i11 = (v5 * matrix.m[0][0] - v2 * matrix.m[0][2] + v1 * matrix.m[0][3]) * inv_det;
			number_t i21 = -(v4 * matrix.m[0][0] - v2 * matrix.m[0][1] + v0 * matrix.m[0][3]) * inv_det;
			number_t i31 = (v3 * matrix.m[0][0] - v1 * matrix.m[0][1] + v0 * matrix.m[0][2]) * inv_det;

			v0 = matrix.m[1][0] * matrix.m[3][1] - matrix.m[1][1] * matrix.m[3][0];
			v1 = matrix.m[1][0] * matrix.m[3][2] - matrix.m[1][2] * matrix.m[3][0];
			v2 = matrix.m[1][0] * matrix.m[3][3] - matrix.m[1][3] * matrix.m[3][0];
			v3 = matrix.m[1][1] * matrix.m[3][2] - matrix.m[1][2] * matrix.m[3][1];
			v4 = matrix.m[1][1] * matrix.m[3][3] - matrix.m[1][3] * matrix.m[3][1];
			v5 = matrix.m[1][2] * matrix.m[3][3] - matrix.m[1][3] * matrix.m[3][2];

			number_t i02 = (v5 * matrix.m[0][1] - v4 * matrix.m[0][2] + v3 * matrix.m[0][3]) * inv_det;
			number_t i12 = -(v5 * matrix.m[0][0] - v2 * matrix.m[0][2] + v1 * matrix.m[0][3]) * inv_det;
			number_t i22 = (v4 * matrix.m[0][0] - v2 * matrix.m[0][1] + v0 * matrix.m[0][3]) * inv_det;
			number_t i32 = -(v3 * matrix.m[0][0] - v1 * matrix.m[0][1] + v0 * matrix.m[0][2]) * inv_det;

			v0 = matrix.m[2][1] * matrix.m[1][0] - matrix.m[2][0] * matrix.m[1][1];
			v1 = matrix.m[2][2] * matrix.m[1][0] - matrix.m[2][0] * matrix.m[1][2];
			v2 = matrix.m[2][3] * matrix.m[1][0] - matrix.m[2][0] * matrix.m[1][3];
			v3 = matrix.m[2][2] * matrix.m[1][1] - matrix.m[2][1] * matrix.m[1][2];
			v4 = matrix.m[2][3] * matrix.m[1][1] - matrix.m[2][1] * matrix.m[1][3];
			v5 = matrix.m[2][3] * matrix.m[1][2] - matrix.m[2][2] * matrix.m[1][3];

			number_t i03 = -(v5 * matrix.m[0][1] - v4 * matrix.m[0][2] + v3 * matrix.m[0][3]) * inv_det;
			number_t i13 = (v5 * matrix.m[0][0] - v2 * matrix.m[0][2] + v1 * matrix.m[0][3]) * inv_det;
			number_t i23 = -(v4 * matrix.m[0][0] - v2 * matrix.m[0][1] + v0 * matrix.m[0][3]) * inv_det;
			number_t i33 = (v3 * matrix.m[0][0] - v1 * matrix.m[0][1] + v0 * matrix.m[0][2]) * inv_det;

			return matrix4x4(
				i00, i01, i02, i03,
				i10, i11, i12, i13,
				i20, i21, i22, i23,
				i30, i31, i32, i33
			);
		}
	}
}