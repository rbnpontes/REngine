#include <iostream>
#include <rengine/rengine.h>

static rengine::core::window_t g_window_id;

void game_loop() {
    if (rengine::core::window_is_destroyed(g_window_id)) {
        rengine::stop();
        return;
    }

    const auto a = rengine::math::vec3(-.5, -.5, 0.);
	const auto b = rengine::math::vec3(+0., +.5, 0.);
	const auto c = rengine::math::vec3(+.5, -.5, 0.);

    rengine::graphics::renderer_begin_draw();
        rengine::graphics::renderer_add_triangle({
            { rengine::math::vec3(-0.5f, -0.5f, 0.), rengine::math::byte_color::red },
            { rengine::math::vec3(+0.0f, +0.5f, 0.), rengine::math::byte_color::green },
            { rengine::math::vec3(+0.5f, -0.5f, 0.), rengine::math::byte_color::blue },
        });
        rengine::graphics::renderer_add_line(a, b);
        rengine::graphics::renderer_add_line(b, c);
        rengine::graphics::renderer_add_line(c, a);
    rengine::graphics::renderer_end_draw();
}

int main(unsigned int argc, char** argv) {   
    rengine::init();
    const auto wnd = rengine::core::window_create("REngine", 500, 400);
    g_window_id = wnd;
    rengine::use_window(wnd);
    rengine::run(game_loop);
    rengine::destroy();
    return 0;
}