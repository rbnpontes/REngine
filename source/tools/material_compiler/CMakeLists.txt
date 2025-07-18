file (GLOB SOURCES *.cpp *.h)

add_executable(material_compiler ${SOURCES})

# Link against engine and slang library
target_link_libraries(material_compiler
    PRIVATE
        rengine
        slang::slang
)

target_compile_options(material_compiler PUBLIC
    -DENGINE_SOURCE_DIR="${ENGINE_SOURCE_DIR}"
    -DENGINE_ROOT_DIR="${ENGINE_ROOT_DIR}"
)

