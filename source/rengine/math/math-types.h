#pragma once
#include <rengine/types.h>
#include <rengine/api.h>

namespace rengine {
    namespace math {
        template<typename T>
        struct base_vec2 {
            T x, y;
            constexpr base_vec2(): x((T)0), y((T)0) {}
            constexpr base_vec2(T x_, T y_): x(x_), y(y_) {}

            bool equal(const base_vec2& target) const {
                return x == target.x && y == target.y;
            }

            bool greater_than(const base_vec2& target) const {
                return x > target.x || y > target.y;
            }
            bool greater_equal_than(const base_vec2& target) const {
                return x >= target.x || y >= target.y;
            }

            bool less_than(const base_vec2& target) const {
                return x < target.x || y < target.y;
            }
            bool less_equal_than(const base_vec2& target) const {
                return x <= target.x || y <= target.y;
            }

            static const base_vec2<T> zero;
            static const base_vec2<T> one;
            static const base_vec2<T> left;
            static const base_vec2<T> right;
            static const base_vec2<T> up;
            static const base_vec2<T> down;
        };

        typedef base_vec2<number_t> vec2;
        typedef base_vec2<int_t> ivec2;
        typedef base_vec2<uint_t> uvec2;

        template<typename T>
        struct base_vec3 {
            T x, y, z;

            constexpr base_vec3(): x((T)0), y((T)0), z((T)0) {}
            constexpr base_vec3(T x_, T y_, T z_) : x(x_), y(y_), z(z_) {}

            static const base_vec3<T> zero;
            static const base_vec3<T> one;
            static const base_vec3<T> left;
            static const base_vec3<T> right;
            static const base_vec3<T> up;
            static const base_vec3<T> down;
            static const base_vec3<T> forward;
            static const base_vec3<T> backward;
        };

        typedef base_vec3<number_t> vec3;
        typedef base_vec3<int_t> ivec3;
        typedef base_vec3<uint_t> uvec3;

        template<typename T>
        struct base_vec4 {
            T x, y, z, w;
        };
        typedef base_vec4<number_t> vec4;
        typedef base_vec4<int_t> ivec4;
        typedef base_vec4<uint_t> uvec4;

        struct quat {
            number_t x, y, z, w;
        };

        struct color {
            float r, g, b, a;
            
            constexpr color(): r(0.), g(0.), b(0.), a(1.){}
            constexpr color(float r_, float g_, float b_, float a_ = 1.0f): r(r_), g(g_), b(b_), a(a_){}

            static const color black;
            static const color white;
            static const color red;
            static const color green;
            static const color blue;
            static const color transparent;
        };

        struct R_EXPORT byte_color {
            u8 r, g, b, a;

            constexpr byte_color(): r(0), g(0), b(0), a(255){}
            constexpr byte_color(u8 r_, u8 g_, u8 b_, u8 a_ = 255): r(r_), g(g_), b(b_), a(a_){}

            static const byte_color black;
            static const byte_color white;
            static const byte_color red;
            static const byte_color green;
            static const byte_color blue;
            static const byte_color transparent;
        };

        template<typename T>
        struct base_rect {
            T position, size;

            constexpr base_rect(): position(T::zero), size(T::zero){}
            constexpr base_rect(T pos, T _size): position(pos), size(_size){}

            bool equal(const base_rect& target) const {
                return position.equal(target.position) && size.equal(target.size);
            }
            static const base_rect<T> zero;
        };
        typedef base_rect<vec2> rect;
        typedef base_rect<ivec2> irect;
        typedef base_rect<uvec2> urect;

        struct box {
            vec3 min, max;
        };
    }
}