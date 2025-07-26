#ifndef ENGINE_MODULE
#define ENGINE_MODULE 1

#define __ENGINE_MODULE__ 1
#include "./modules/_utils.module.slang"

#ifdef __VERTEX__
    #include "./modules/_vertex.module.slang"
#endif

#ifdef __PIXEL__
    #include "./modules/_pixel.module.slang"
#endif

#include "./modules/_materials.module.slang"

#endif