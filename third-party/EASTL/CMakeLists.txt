#-------------------------------------------------------------------------------------------
# Copyright (C) Electronic Arts Inc.  All rights reserved.
#-------------------------------------------------------------------------------------------
cmake_minimum_required(VERSION 3.11)
project(EASTL CXX)

#-------------------------------------------------------------------------------------------
# Compiler Flags
#-------------------------------------------------------------------------------------------
set (CMAKE_MODULE_PATH "${CMAKE_MODULE_PATH};${CMAKE_CURRENT_SOURCE_DIR}/CMake")
include(CommonCppFlags)

#-------------------------------------------------------------------------------------------
# Library definition
#-------------------------------------------------------------------------------------------
file(GLOB EASTL_SOURCES "source/*.cpp")
file(GLOB_RECURSE EASTL_HEADERS "include/EASTL/**.h")
add_library(EASTL ${EASTL_SOURCES} ${EASTL_HEADERS})
target_compile_features(EASTL PUBLIC cxx_std_14)

# include both source and headers in the files tab in Visual Studio
source_group(TREE ${CMAKE_CURRENT_SOURCE_DIR} PREFIX "Header Files" FILES ${EASTL_HEADERS})

if (MSVC)
    set(EASTL_NATVIS_FILE "EASTL.natvis")
    target_sources(EASTL INTERFACE
        $<INSTALL_INTERFACE:${EASTL_NATVIS_FILE}>
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/${EASTL_NATVIS_FILE}>
    )
endif()

#-------------------------------------------------------------------------------------------
# Defines
#-------------------------------------------------------------------------------------------
add_definitions(-D_CHAR16T)
add_definitions(-D_CRT_SECURE_NO_WARNINGS)
add_definitions(-D_SCL_SECURE_NO_WARNINGS)
add_definitions(-DEASTL_OPENSOURCE=1)
add_definitions(-DEASTL_RTTI_ENABLED=0)
add_definitions(-DEASTL_STD_ITERATOR_CATEGORY_ENABLED=1)
add_definitions(-DEASTDC_GLOBALPTR_SUPPORT_ENABLED=0)
add_definitions(-DEASTL_DEBUG=$<OR:$<CONFIG:Debug>,$<CONFIG:RelWithDebInfo>>)

#-------------------------------------------------------------------------------------------
# Include dirs
#-------------------------------------------------------------------------------------------
target_include_directories(EASTL PUBLIC include)

#-------------------------------------------------------------------------------------------
# Dependencies
#-------------------------------------------------------------------------------------------
add_subdirectory(packages/EABase)
target_link_libraries(EASTL EABase)

#-------------------------------------------------------------------------------------------
# Deprecations
#-------------------------------------------------------------------------------------------
if(EASTL_DISABLE_APRIL_2024_DEPRECATIONS)
  target_compile_definitions(EASTL PUBLIC EA_DEPRECATIONS_FOR_2024_APRIL=EA_DISABLED)
endif()
if(EASTL_DISABLE_SEPT_2024_DEPRECATIONS)
  target_compile_definitions(EASTL PUBLIC EA_DEPRECATIONS_FOR_2024_SEPT=EA_DISABLED)
endif()
if(EASTL_DISABLE_APRIL_2025_DEPRECATIONS)
  target_compile_definitions(EASTL PUBLIC EA_DEPRECATIONS_FOR_2025_APRIL=EA_DISABLED)
endif()


#-------------------------------------------------------------------------------------------
# Installation
#-------------------------------------------------------------------------------------------
install(TARGETS EASTL DESTINATION lib)
install(DIRECTORY include/EASTL DESTINATION include)