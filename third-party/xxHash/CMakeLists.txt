file (GLOB SOURCE_FILES src/xxHash/*.c src/xxHash/*.h)

set (LIB_DEFINITIONS)

if("${CMAKE_BUILD_TYPE}" STREQUAL "Debug")
    list(APPEND LIB_DEFINITIONS -DXXH_DEBUGLEVEL=1)
endif()

add_library(xxHash STATIC ${SOURCE_FILES})
set_target_properties(xxHash PROPERTIES LINKER_LANGUAGE CXX)
target_include_directories(xxHash PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/src)