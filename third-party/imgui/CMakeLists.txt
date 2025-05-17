file (GLOB HEADER_FILES src/imgui/*.h)
file (GLOB SOURCE_FILES src/imgui/*.cpp)

add_library(imgui STATIC ${HEADER_FILES} ${SOURCE_FILES})

if(MSVC)
    set(NATVIS_FILE "debuggers/imgui.natvis")
    target_sources(imgui PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/${NATVIS_FILE})
endif()

target_link_libraries(imgui PUBLIC EASTL)
target_include_directories(imgui PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/src)
