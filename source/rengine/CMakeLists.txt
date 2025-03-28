file (GLOB SOURCE_FILES *.cpp **/*.cpp)
file (GLOB PUBLIC_HEADER_FILES *.h **/*.h)
file (GLOB PRIVATE_HEADER_FILES *_private.h **/*_private.h)

# Exclude private headers from the public header list
list(FILTER PUBLIC_HEADER_FILES EXCLUDE REGEX ".*_private.h")

function(assign_source_groups)
    foreach(FILE_PATH ${ARGN})
        # Normalize path and make it relative
        file(RELATIVE_PATH REL_PATH "${CMAKE_CURRENT_SOURCE_DIR}" "${FILE_PATH}")
        
        # Extract directory structure for grouping
        get_filename_component(PARENT_DIR "${REL_PATH}" DIRECTORY)
        
        # Convert '/' to '\\' for Windows (Visual Studio requirement)
        string(REPLACE "/" "\\" GROUP_NAME "${PARENT_DIR}")

        # Assign file to its group in Visual Studio
        source_group("${GROUP_NAME}" FILES "${FILE_PATH}")
    endforeach()
endfunction()

assign_source_groups(${SOURCE_FILES})
assign_source_groups(${PUBLIC_HEADER_FILES})
assign_source_groups(${PRIVATE_HEADER_FILES})

add_library(rengine SHARED ${SOURCE_FILES} ${PUBLIC_HEADER_FILES} ${PRIVATE_HEADER_FILES})

set (PRIVATE_LIBS
    Diligent-BuildSettings
    Diligent-Common
    Diligent-GraphicsEngineOpenGL-static
    Diligent-GraphicsEngineVk-static
    fmt::fmt
    SDL3::SDL3-static
    EASTL
    EABase
)

if(NOT DILIGENT_NO_WEBGPU)
    list (APPEND PRIVATE_LIBS
        Diligent-GraphicsEngineWebGPU-static
    )
endif()

if(WIN32)
    list (APPEND PRIVATE_LIBS
        Diligent-GraphicsEngineD3D11-static
        Diligent-GraphicsEngineD3D12-static
    )
endif()

target_link_libraries(rengine PRIVATE ${PRIVATE_LIBS})
target_compile_options(rengine PRIVATE -DENGINE_LIB)
target_include_directories(rengine PUBLIC ${ENGINE_SOURCE_DIR})

# Diligent required call
if(WIN32)
    copy_required_dlls(rengine)
endif()