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

    const auto a_color = rengine::math::byte_color::red;
	const auto b_color = rengine::math::byte_color::green;
	const auto c_color = rengine::math::byte_color::blue;

    rengine::graphics::renderer_begin_draw();
    {
        rengine::graphics::renderer_set_color(a_color);
        rengine::graphics::renderer_push_vertex(a);

        rengine::graphics::renderer_set_color(b_color);
        rengine::graphics::renderer_push_vertex(b);

        rengine::graphics::renderer_set_color(c_color);
        rengine::graphics::renderer_push_vertex(c);

        rengine::graphics::renderer_draw_triangle();
    }

    {
        // Draw line: A -> B
        {
		    rengine::graphics::renderer_set_color(a_color);
		    rengine::graphics::renderer_push_vertex(a);

		    rengine::graphics::renderer_set_color(b_color);
		    rengine::graphics::renderer_push_vertex(b);

		    rengine::graphics::renderer_draw_line();
        }

		// Draw line: B -> C
        {
		    rengine::graphics::renderer_set_color(b_color);
		    rengine::graphics::renderer_push_vertex(b);

		    rengine::graphics::renderer_set_color(c_color);
		    rengine::graphics::renderer_push_vertex(c);

		    rengine::graphics::renderer_draw_line();
        }

        // Draw line: C -> A
        {
		    rengine::graphics::renderer_set_color(c_color);
		    rengine::graphics::renderer_push_vertex(c);

		    rengine::graphics::renderer_set_color(a_color);
		    rengine::graphics::renderer_push_vertex(a);

		    rengine::graphics::renderer_draw_line();
        }
    }

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