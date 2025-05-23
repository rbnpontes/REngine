#include "./math-types.h"

namespace rengine {
	namespace math {
        template struct base_vec2<number_t>;
        template struct base_vec2<int_t>;
        template struct base_vec2<uint_t>;

        const color color::black = color(0.f, 0.f, 0.f);
        const color color::white = color(1.f, 1.f, 1.f);
        const color color::red = color(1.f, 0.f, 0.f);
        const color color::green = color(0.f, 1.f, 0.f);
        const color color::blue = color(0.f, 0.f, 1.f);
        const color color::transparent = color(0.f, 0.f, 0.f, 0.f);

        const byte_color byte_color::black = byte_color();
        const byte_color byte_color::white = byte_color(255, 255, 255);
        const byte_color byte_color::red = byte_color(255, 0, 0);
        const byte_color byte_color::green = byte_color(0, 255, 0);
        const byte_color byte_color::blue = byte_color(0, 0, 255);
        const byte_color byte_color::yellow = byte_color(255, 255, 0);
        const byte_color byte_color::transparent = byte_color(0, 0, 0, 0);

        template <typename T> const base_rect<T> base_rect<T>::zero = base_rect<T>(T::zero, T::zero);
        
        template struct base_rect<vec2>;
        template struct base_rect<ivec2>;
        template struct base_rect<uvec2>;
	}
}