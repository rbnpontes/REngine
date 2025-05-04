file (GLOB HEADER_FILES include/stb/*.h)

add_library(stb STATIC ${HEADER_FILES} dummy.cpp)
target_include_directories(stb PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include)