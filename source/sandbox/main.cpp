#include <iostream>
#include <rengine/rengine.h>

struct Foo {
    int a;
    int b;
};

void game_loop() {

}

int main(unsigned int argc, char** argv) {
    rengine::init();
    const auto wnd = rengine::core::window_create("REngine", 400, 500);
    rengine::run(game_loop);
    rengine::destroy();
    return 0;
}