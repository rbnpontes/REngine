#pragma once
#include <rengine/defines.h>
#include <rengine/types.h>
#include <math.h>

namespace rengine {
	namespace math {
		template <typename T>
		inline T abs(T x);

		template<> inline float abs(float x) { return fabsf(x); }
		template<> inline double abs(double x) { return fabs(x); }
		template<> inline int abs(int x) { return x < 0 ? -x : x; }
		template<> inline short abs(short x) { return x < 0 ? -x : x; }
		template<> inline long abs(long x) { return x < 0 ? -x : x; }

		template <typename T>
		inline T sqrt(T x);

		template<> inline float sqrt(float x) { return sqrtf(x); }
		template<> inline double sqrt(double x) { return ::sqrt(x); }
		template<> inline int sqrt(int x) { return static_cast<int>(sqrtf(static_cast<float>(x))); }
		template<> inline short sqrt(short x) { return static_cast<short>(sqrtf(static_cast<float>(x))); }
		template<> inline long sqrt(long x) { return static_cast<long>(sqrt(static_cast<double>(x))); }

		template <typename T>
		inline bool equals(T a, T b);

		template<> inline bool equals(float a, float b) { return abs(a - b) <= MATH_EPSILON; }
		template<> inline bool equals(double a, double b) { return abs(a - b) <= MATH_EPSILON; }
		template<> inline bool equals(int a, int b) { return a == b; }
		template<> inline bool equals(short a, short b) { return a == b; }
		template<> inline bool equals(long a, long b) { return a == b; }
		template<> inline bool equals(u8 a, u8 b) { return a == b; }
		template<> inline bool equals(u16 a, u16 b) { return a == b; }
		template<> inline bool equals(u32 a, u32 b) { return a == b; }
		template<> inline bool equals(u64 a, u64 b) { return a == b; }

		inline bool equals_ptr(const ptr a, const ptr b) { return a == b; }
	}
}