#pragma once
#include <rengine/types.h>
#include <rengine/math/sse.h>
#include <rengine/math/math-operations.h>
#include <rengine/core/hash.h>

namespace rengine {
	namespace math {
		template<typename T>
		struct base_vec3 {
			T x, y, z;

			constexpr base_vec3() : x((T)0), y((T)0), z((T)0) {}
			constexpr base_vec3(T x_, T y_, T z_ = (T)0) : x(x_), y(y_), z(z_) {}
			constexpr base_vec3(const T* data) : x(data[0]), y(data[1]), z(data[2]) {}

			u32 to_hash() const {
				return core::hash_combine(core::hash_combine(x, y), z);
			}

			bool equals(const base_vec3& other) const {
				return math::equals(x, other.x) && math::equals(y, other.y) && math::equals(z, other.z);
			}

			bool operator ==(const base_vec3<T>& other) {
				return equals(other);
			}
			base_vec3<T> operator +() const {
				return base_vec3<T>(+x, +y, +z);
			}
			base_vec3<T> operator +(const base_vec3<T>& other) const {
				return add(*this, other);
			}
			base_vec3<T> operator +(T other) const {
				return base_vec3<T>(x + other, y + other, z + other);
			}
			base_vec3<T> operator -() const {
				return base_vec3<T>(-x, -y, -z);
			}
			base_vec3<T> operator -(const base_vec3<T>& other) const {
				return base_vec3<T>(x - other.x, y - other.y, z - other.z);
			}
			base_vec3<T> operator -(T other) const {
				return base_vec3<T>(x - other, y - other, z - other);
			}
			base_vec3<T> operator *(const base_vec3<T>& other) const {
				return base_vec3<T>(x * other.x, y * other.y, z * other.z);
			}
			base_vec3<T> operator *(T other) const {
				return base_vec3<T>(x * other, y * other, z * other);
			}
			base_vec3<T> operator /(T other) const {
				return base_vec3<T>(x / other, y / other, z / other);
			}
			base_vec3<T> operator /(const base_vec3<T>& other) const {
				return base_vec3<T>(x / other.x, y / other.y, z / other.z);
			}


			base_vec3<T>& operator +=(const base_vec3<T>& other) {
				x += other.x;
				y += other.y;
				z += other.z;
				return *this;
			}
			base_vec3<T>& operator -=(const base_vec3<T>& other) {
				x -= other.x;
				y -= other.y;
				z -= other.z;
				return *this;
			}
			base_vec3<T>& operator *=(const base_vec3<T>& other) {
				x *= other.x;
				y *= other.y;
				z *= other.z;
				return *this;
			}
			base_vec3<T>& operator /=(const base_vec3<T>& other) {
				x /= other.x;
				y /= other.y;
				z /= other.z;
				return *this;
			}

			T* data() {
				return &x;
			}

			static base_vec3<T> add(const base_vec3<T>& a, const base_vec3<T>& b);
			static base_vec3<T> add(const base_vec3<T>& a, T b);

			static T length(const base_vec3<T>& vec) {
				return (T)math::sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
			}
			static T length_squared(const base_vec3<T>& vec) {
				return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z;
			}
			static base_vec3<T> normalize(const base_vec3<T>& vec) {
				number_t len = length_squared(vec);
				if (math::equals<T>(len, 1.) || len < 0.)
					return vec;

				number_t inv_len = 1. / math::sqrt(len);
				return base_vec3<T>(vec.x * inv_len, vec.y * inv_len, vec.z * inv_len);
			}
			static T dot(const base_vec3<T>& a, const base_vec3<T>& b) {
				return a.x * b.x + a.y * b.y + a.z * b.z;
			}
			static T dot_abs(const base_vec3<T>& a, const base_vec3<T>& b) {
				return math::abs(a.x * b.x) + math::abs(a.y * b.y) + math::abs(a.z * b.z);
			}
			static T project_axis(const base_vec3<T>& vec, const base_vec3<T>& axis) {
				return dot(vec, normalize(axis));
			}
			static base_vec3<T> project_plane(const base_vec3<T>& vec, const base_vec3<T>& origin, const base_vec3<T>& normal) {
				const auto delta = vec - origin;
				return vec - normalize(normal) * project_axis(delta, normal);
			}
			static base_vec3<T> project_line(const base_vec3<T>& vec, const base_vec3<T>& from, const base_vec3<T>& to, bool clamped = false) {
				const auto dir = to - from;
				auto factor = dot(vec - from, dir) / length_squared(dir);

				if (clamped)
					factor = math::clamp<T>(factor, 0., 1.);
				return from + dir * factor;
			}
			static T distance(const base_vec3<T>& a, const base_vec3<T>& b) {
				return length(b - a);
			}
			static T distance_plane(const base_vec3<T>& vec, const base_vec3<T>& origin, const base_vec3<T>& normal) {
				return project_axis(vec - origin, normal);
			}
			static base_vec3<T> cross(const base_vec3<T>& a, const base_vec3<T>& b) {
				return base_vec3<T>(
					a.y * b.z - a.z * b.y,
					a.z * b.x - a.x * b.z,
					a.x * b.y - a.y * b.x
				);
			}
			static base_vec3<T> ortho(const base_vec3<T>& vec, const base_vec3<T>& axis) {
				return normalize(
					cross(
						cross(axis, vec),
						axis
					)
				);
			}

			static base_vec3<T> from_x(T x) {
				return base_vec3<T>(x, 0, 0);
			}
			static base_vec3<T> from_y(T y) {
				return base_vec3<T>(0, y, 0);
			}
			static base_vec3<T> from_z(T x) {
				return base_vec3<T>(0, 0, x);
			}

			static const base_vec3<T> zero;
			static const base_vec3<T> one;
			static const base_vec3<T> left;
			static const base_vec3<T> right;
			static const base_vec3<T> up;
			static const base_vec3<T> down;
			static const base_vec3<T> forward;
			static const base_vec3<T> backward;
		};

		template<typename T> inline const base_vec3<T> base_vec3<T>::zero = base_vec3<T>((T)0, (T)0, (T)0);
		template<typename T> inline const base_vec3<T> base_vec3<T>::one = base_vec3<T>((T)1, (T)1, (T)1);
		template<typename T> inline const base_vec3<T> base_vec3<T>::left = base_vec3<T>((T)-1, (T)0, (T)0);
		template<typename T> inline const base_vec3<T> base_vec3<T>::right = base_vec3<T>((T)1, (T)0, (T)0);
		template<typename T> inline const base_vec3<T> base_vec3<T>::up = base_vec3<T>((T)0, (T)1, (T)0);
		template<typename T> inline const base_vec3<T> base_vec3<T>::down = base_vec3<T>((T)0, (T)-1, (T)0);
		template<typename T> inline const base_vec3<T> base_vec3<T>::forward = base_vec3<T>((T)0, (T)0, (T)1);
		template<typename T> inline const base_vec3<T> base_vec3<T>::backward = base_vec3<T>((T)0, (T)0, (T)-1);

		typedef base_vec3<number_t> vec3;
		typedef base_vec3<int_t> ivec3;
		typedef base_vec3<uint_t> uvec3;

		inline vec3 base_vec3<number_t>::add(const vec3& a, const vec3& b) {
			auto first = sse_set_number(0, a.z, a.y, a.x);
			auto second = sse_set_number(0, b.z, b.y, b.x);

			first = sse_add_number(first, second);

			number_t result[4];
			sse_store_number(result, first);
			return vec3(result);
		}
		inline vec3 base_vec3<number_t>::add(const vec3& a, number_t b) {
			auto first = sse_set_number(0, a.z, a.y, a.x);
			const auto second = sse_set_single_number(b);

			first = sse_add_number(first, second);

			number_t result[4];
			sse_store_number(result, first);
			return vec3(result);
		}

		inline ivec3 base_vec3<int_t>::add(const ivec3& a, const ivec3& b) {
			auto first = sse_set_number(0, (number_t)a.z, (number_t)a.y, (number_t)a.x);
			auto second = sse_set_number(0, (number_t)b.z, (number_t)b.y, (number_t)b.x);

			first = sse_add_number(first, second);

			number_t result[4];
			sse_store_number(result, first);
			return ivec3((int_t)result[0], (int_t)result[1], (int_t)result[2]);
		}
		inline ivec3 base_vec3<int_t>::add(const ivec3& a, int_t b) {
			auto first = sse_set_number(0, a.z, a.y, a.x);
			const auto second = sse_set_single_number((number_t)b);

			first = sse_add_number(first, second);

			number_t result[4];
			sse_store_number(result, first);
			return ivec3((int_t)result[0], (int_t)result[1], (int_t)result[2]);
		}

		inline uvec3 base_vec3<uint_t>::add(const uvec3& a, const uvec3& b) {
			auto first = sse_set_number(0, (number_t)a.z, (number_t)a.y, (number_t)a.x);
			auto second = sse_set_number(0, (number_t)b.z, (number_t)b.y, (number_t)b.x);

			first = sse_add_number(first, second);

			number_t result[4];
			sse_store_number(result, first);
			return uvec3((uint_t)result[0], (uint_t)result[1], (uint_t)result[2]);
		}
		inline uvec3 base_vec3<uint_t>::add(const uvec3& a, uint_t b) {
			auto first = sse_set_number(0, a.z, a.y, a.x);
			const auto second = sse_set_single_number((number_t)b);

			first = sse_add_number(first, second);

			number_t result[4];
			sse_store_number(result, first);
			return uvec3((uint_t)result[0], (uint_t)result[1], (uint_t)result[2]);
		}

		template<> inline vec3 abs(vec3 x) { return vec3(math::abs(x.x), math::abs(x.y), math::abs(x.z)); }
		template<> inline ivec3 abs(ivec3 x) { return ivec3(math::abs(x.x), math::abs(x.y), math::abs(x.z)); }
		template<> inline uvec3 abs(uvec3 x) { return uvec3(math::abs(x.x), math::abs(x.y), math::abs(x.z)); }

		template<> inline vec3 lerp(vec3 from, vec3 to, float t) {
			return vec3(
				math::lerp(from.x, to.x, t),
				math::lerp(from.y, to.y, t),
				math::lerp(from.z, to.z, t)
			);
		}
		template<> inline vec3 lerp(vec3 from, vec3 to, double t) {
			return vec3(
				math::lerp(from.x, to.x, t),
				math::lerp(from.y, to.y, t),
				math::lerp(from.z, to.z, t)
			);
		}
		template<> inline ivec3 lerp(ivec3 from, ivec3 to, float t) {
			return ivec3(
				math::lerp(from.x, to.x, t),
				math::lerp(from.y, to.y, t),
				math::lerp(from.z, to.z, t)
			);
		}
		template<> inline ivec3 lerp(ivec3 from, ivec3 to, double t) {
			return ivec3(
				math::lerp(from.x, to.x, t),
				math::lerp(from.y, to.y, t),
				math::lerp(from.z, to.z, t)
			);
		}
	}
}
