file (GLOB SOURCES *.cpp *.h)

add_executable(sandbox ${SOURCES})
target_link_libraries(sandbox rengine)

target_compile_options(sandbox PUBLIC 
    -DENGINE_SOURCE_DIR="${ENGINE_SOURCE_DIR}"
    -DENGINE_ROOT_DIR="${ENGINE_ROOT_DIR}")