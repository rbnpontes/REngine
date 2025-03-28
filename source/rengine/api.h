#pragma once
#ifdef ENGINE_LIB
    #define R_EXPORT __declspec(dllexport)
    #define PRIVATE_HEADER
#else
    #define R_EXPORT __declspec(dllimport)
    #define PRIVATE_HEADER "It seems that you are trying to include a private header file."
#endif