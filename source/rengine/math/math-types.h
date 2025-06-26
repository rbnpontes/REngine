#pragma once
#include <rengine/types.h>
#include <rengine/api.h>
#include <rengine/core/hash.h>
#include <rengine/math/sse.h>


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

            u32 to_hash() const {
                return core::hash_combine(x, y);
            }

            static const base_vec2<T> zero;
            static const base_vec2<T> one;
            static const base_vec2<T> left;
            static const base_vec2<T> right;
            static const base_vec2<T> up;
            static const base_vec2<T> down;
        };

        template<typename T> inline const base_vec2<T> base_vec2<T>::zero = base_vec2<T>((T)+0, (T)+0);
        template<typename T> inline const base_vec2<T> base_vec2<T>::one = base_vec2<T>((T)+1, (T)+1);
        template<typename T> inline const base_vec2<T> base_vec2<T>::left = base_vec2<T>((T)-1, (T)+0);
        template<typename T> inline const base_vec2<T> base_vec2<T>::right = base_vec2<T>((T)+1, (T)+0);
        template<typename T> inline const base_vec2<T> base_vec2<T>::up = base_vec2<T>((T)+0, (T)+1);
        template<typename T> inline const base_vec2<T> base_vec2<T>::down = base_vec2<T>((T)+0, (T)-1);

        typedef base_vec2<number_t> vec2;
        typedef base_vec2<int_t> ivec2;
        typedef base_vec2<uint_t> uvec2;

        template<typename T>
        struct base_vec4 {
            T x, y, z, w;
        };
        typedef base_vec4<number_t> vec4;
        typedef base_vec4<int_t> ivec4;
        typedef base_vec4<uint_t> uvec4;

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
            constexpr byte_color(u32 color): r((color >> 16) & 0xFF), g((color >> 8) & 0xFF), b((color >> 0) & 0xFF), a((color >> 24) & 0xFF){}
            constexpr byte_color(u8* data, u8 num_components): byte_color() {
                u8* dst = (u8*)this;
                for (u8 i = 0; i < num_components; ++i) {
					*dst = data[i];
                    ++dst;
                }
            }

            constexpr u32 to_uint() {
                return (a << 24u) | (r << 16u) | (g << 8u) | b;
            }
			constexpr color to_color() const {
				return color(r / 255.f, g / 255.f, b / 255.f, a / 255.f);
			}

            static byte_color from(color col) {
				return byte_color(col.r * 255.f, col.g * 255.f, col.b * 255.f, col.a * 255.f);
            }

            static const byte_color black;
            static const byte_color white;
            static const byte_color red;
            static const byte_color green;
            static const byte_color blue;
            static const byte_color yellow;
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

            u32 to_hash() const {
                return core::hash_combine(position.to_hash(), size.to_hash());
            }

            static const base_rect<T> zero;
        };
        typedef base_rect<vec2> rect;
        typedef base_rect<ivec2> irect;
        typedef base_rect<uvec2> urect;
    }
}