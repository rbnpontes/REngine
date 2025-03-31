#include <iostream>
#include <rengine/rengine.h>

static rengine::core::window_t g_window_id;

void game_loop() {
    if (rengine::core::window_is_destroyed(g_window_id)) {
        rengine::stop();
        return;
    }

}

int main(unsigned int argc, char** argv) {
    rengine::init();
    const auto wnd = rengine::core::window_create("REngine", 500, 400);
    g_window_id = wnd;
    rengine::run(game_loop);
    rengine::destroy();
    return 0;
}