#include <iostream>
#include <slang.h>
#include <rengine/rengine.h>

int main(int argc, char** argv)
{
    rengine::init();
    std::cout << "Slang version: " << spGetBuildTagString() << std::endl;
    rengine::destroy();
    return 0;
}

