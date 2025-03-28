project(REngine C CXX)

set (ENGINE_SOURCE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/source)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

add_subdirectory(third-party)
add_subdirectory(source)