file (GLOB SOURCES *.cpp *.h)

add_executable(sandbox ${SOURCES})
target_link_libraries(sandbox rengine)