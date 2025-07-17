#pragma once
#include <rengine/defines.h>
#include <rengine/types.h>
#include <math.h>

#undef min
#undef max
namespace rengine {
	namespace math {
		template <typename T>
		inline T abs(T x);

		template<> inline float abs(float x) { return fabsf(x); }
		template<> inline double abs(double x) { return fabs(x); }
		template<> inline int abs(int x) { return x < 0 ? -x : x; }
		template<> inline short abs(short x) { return x < 0 ? -x : x; }
		template<> inline long abs(long x) { return x < 0 ? -x : x; }
		template<> inline u32 abs(u32 x) { return x; }

		template <typename T>
		inline T sqrt(T x);

		template<> inline float sqrt(float x) { return sqrtf(x); }
		template<> inline double sqrt(double x) { return ::sqrt(x); }
		template<> inline int sqrt(int x) { return static_cast<int>(sqrtf(static_cast<float>(x))); }
		template<> inline short sqrt(short x) { return static_cast<short>(sqrtf(static_cast<float>(x))); }
		template<> inline long sqrt(long x) { return static_cast<long>(sqrt(static_cast<double>(x))); }
		template<> inline u32 sqrt(u32 x) { return ::sqrt((double)x); }

		template <typename T>
		inline T sin(T x);
		template <> inline float sin(float x) { return ::sinf(x); }
		template <> inline double sin(double x) { return ::sin(x); }

		template <typename T>
		inline T cos(T x);
		template <> inline float cos(float x) { return ::cosf(x); }
		template <> inline double cos(double x) { return ::cos(x); }

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

		template <typename T>
		inline T min(T value, T min) { return value < min ? min : value; }
		template <typename T>
		inline T max(T value, T max) { return value > max ? max : value; }

		template <typename T>
		inline T clamp(T value, T min_, T max_) {
			//return (value < min_) ? min_ : (value > max_) ? max_ : value;
			return math::min(min_, math::max(max_, value));
		}

		template <typename T, typename Time>
		inline T lerp(T from, T to, Time t);

		template<> inline float lerp(float from, float to, float t) { return (1. - t) * from + t * to; }
		template<> inline double lerp(double from, double to, double t) { return (1. - t) * from + t * to; }
		template<> inline short lerp(short from, short to, float t) { return (short)lerp((float)from, (float)to, t); }
		template<> inline int lerp(int from, int to, float t) { return (int)lerp((double)from, (double)to, t); }
		template<> inline long lerp(long from, long to, double t) { return (long)lerp((double)from, (double)to, t); }

		template <typename T>
		inline T is_nan(T x);

		template<> inline float is_nan(float x) { return isnan(x); }
		template<> inline double is_nan(double x) { return isnan(x); }

		template <typename T>
		inline T is_inf(T x);

		template<> inline float is_inf(float x) { return isinf(x); }
		template<> inline double is_inf(double x) { return isinf(x); }

		template <typename T>
		inline bool is_power_two(T x);

		template<> inline bool is_power_two(byte x) { return x != 0 && (x & (x - 1)) == 0; }
		template<> inline bool is_power_two(char x) { return x != 0 && (x & (x - 1)) == 0; }
		template<> inline bool is_power_two(long x) { return x != 0 && (x & (x - 1)) == 0; }
		template<> inline bool is_power_two(u64 x) { return x != 0 && (x & (x - 1)) == 0; }
		template<> inline bool is_power_two(float x) { return is_power_two((long)x); }
		template<> inline bool is_power_two(double x) { return is_power_two((long)x); }
	}
}