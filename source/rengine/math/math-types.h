#pragma once
#include <rengine/types.h>

namespace rengine {
    namespace math {
        template<typename T>
        struct base_vec2 {
            T x, y;
            constexpr base_vec2(): x((T)0), y((T)0) {}
            constexpr base_vec2(T x_, T y_): x(x_), y(y_) {}

            static const base_vec2<T> zero;
            static const base_vec2<T> one;
            static const base_vec2<T> left;
            static const base_vec2<T> right;
            static const base_vec2<T> up;
            static const base_vec2<T> down;
        };
        template<typename T> const base_vec2<T> base_vec2<T>::zero  = base_vec2<T>((T)+0, (T)+0);
        template<typename T> const base_vec2<T> base_vec2<T>::one   = base_vec2<T>((T)+1, (T)+1);
        template<typename T> const base_vec2<T> base_vec2<T>::left  = base_vec2<T>((T)-1, (T)+0);
        template<typename T> const base_vec2<T> base_vec2<T>::right = base_vec2<T>((T)+1, (T)+0);
        template<typename T> const base_vec2<T> base_vec2<T>::up    = base_vec2<T>((T)+0, (T)+1);
        template<typename T> const base_vec2<T> base_vec2<T>::down  = base_vec2<T>((T)+0, (T)-1);

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
        template<typename T> const base_vec3<T> base_vec3<T>::zero      = base_vec3<T>((T)+0, (T)+0, (T)+0);
        template<typename T> const base_vec3<T> base_vec3<T>::one       = base_vec3<T>((T)+1, (T)+1, (T)+1);
        template<typename T> const base_vec3<T> base_vec3<T>::left      = base_vec3<T>((T)-1, (T)+0, (T)+0);
        template<typename T> const base_vec3<T> base_vec3<T>::right     = base_vec3<T>((T)+1, (T)+0, (T)+0);
        template<typename T> const base_vec3<T> base_vec3<T>::up        = base_vec3<T>((T)+0, (T)+1, (T)+0);
        template<typename T> const base_vec3<T> base_vec3<T>::down      = base_vec3<T>((T)+0, (T)-1, (T)+0);
        template<typename T> const base_vec3<T> base_vec3<T>::forward   = base_vec3<T>((T)+0, (T)+0, (T)+1);
        template<typename T> const base_vec3<T> base_vec3<T>::backward  = base_vec3<T>((T)+0, (T)+0, (T)-1);

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
        const color color::black = color();
        const color color::white = color(1., 1., 1.);
        const color color::red = color(1., 0., 0.);
        const color color::green = color(0., 1., 0.);
        const color color::blue = color(0., 0., 1.);
        const color color::transparent = color(0., 0., 0., 0.);


        template<typename T>
        struct base_rect {
            T position, size;
        };
        typedef base_rect<vec2> rect;
        typedef base_rect<ivec2> irect;
        typedef base_rect<uvec2> urect;

        struct box {
            vec3 min, max;
        };
    }
}