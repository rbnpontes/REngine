cmake_minimum_required(VERSION 3.10)

# Run version helper script
include(cmake/version.cmake)

project(Tracy LANGUAGES CXX VERSION ${TRACY_VERSION_STRING})

file(GENERATE OUTPUT .gitignore CONTENT "*")

set (TRACY_STATIC FALSE)

find_package(Threads REQUIRED)

set(TRACY_PUBLIC_DIR ${CMAKE_CURRENT_SOURCE_DIR}/public)
set(TRACY_MANUAL_LIFETIME TRUE)
set(TRACY_VISIBILITY "SHARED")

add_library(TracyClient ${TRACY_VISIBILITY} "${TRACY_PUBLIC_DIR}/TracyClient.cpp")
target_compile_features(TracyClient PUBLIC cxx_std_11)
target_include_directories(TracyClient PUBLIC
    $<BUILD_INTERFACE:${TRACY_PUBLIC_DIR}>
    $<INSTALL_INTERFACE:include>)
target_link_libraries(
    TracyClient
    PUBLIC
        Threads::Threads
        ${CMAKE_DL_LIBS}
)

# Public dependency on some libraries required when using Mingw
if(WIN32 AND ${CMAKE_CXX_COMPILER_ID} MATCHES "GNU|Clang")
    target_link_libraries(TracyClient PUBLIC ws2_32 dbghelp)
endif()

if(CMAKE_SYSTEM_NAME MATCHES "FreeBSD")
    find_library(EXECINFO_LIBRARY NAMES execinfo REQUIRED)
    target_link_libraries(TracyClient PUBLIC ${EXECINFO_LIBRARY})
endif()

if(TRACY_LIBUNWIND_BACKTRACE)
    include(FindPkgConfig)
    pkg_check_modules(unwind REQUIRED libunwind)
    target_include_directories(TracyClient INTERFACE ${unwind_INCLUDE_DIRS})
    target_link_libraries(TracyClient INTERFACE ${unwind_LINK_LIBRARIES})
endif()

add_library(Tracy::TracyClient ALIAS TracyClient)

macro(set_option option help value)
    option(${option} ${help} ${value})
    if(${option})
        message(STATUS "${option}: ON")
        target_compile_definitions(TracyClient PUBLIC ${option})
    else()
        message(STATUS "${option}: OFF")
    endif()
endmacro()

set (TRACY_ENABLE TRUE)
set (TRACY_DELAYED_INIT TRUE)
set_option(TRACY_ON_DEMAND "On-demand profiling" OFF)
set_option(TRACY_CALLSTACK "Enforce callstack collection for tracy regions" OFF)
set_option(TRACY_NO_CALLSTACK "Disable all callstack related functionality" OFF)
set_option(TRACY_NO_CALLSTACK_INLINES "Disables the inline functions in callstacks" OFF)
set_option(TRACY_ONLY_LOCALHOST "Only listen on the localhost interface" OFF)
set_option(TRACY_NO_BROADCAST "Disable client discovery by broadcast to local network" OFF)
set_option(TRACY_ONLY_IPV4 "Tracy will only accept connections on IPv4 addresses (disable IPv6)" OFF)
set_option(TRACY_NO_CODE_TRANSFER "Disable collection of source code" OFF)
set_option(TRACY_NO_CONTEXT_SWITCH "Disable capture of context switches" OFF)
set_option(TRACY_NO_EXIT "Client executable does not exit until all profile data is sent to server" OFF)
set_option(TRACY_NO_SAMPLING "Disable call stack sampling" OFF)
set_option(TRACY_NO_VERIFY "Disable zone validation for C API" OFF)
set_option(TRACY_NO_VSYNC_CAPTURE "Disable capture of hardware Vsync events" OFF)
set_option(TRACY_NO_FRAME_IMAGE  "Disable the frame image support and its thread" OFF)
set_option(TRACY_NO_SYSTEM_TRACING  "Disable systrace sampling" OFF)
set_option(TRACY_PATCHABLE_NOPSLEDS  "Enable nopsleds for efficient patching by system-level tools (e.g. rr)" OFF)
set_option(TRACY_FIBERS "Enable fibers support" OFF)
set_option(TRACY_NO_CRASH_HANDLER "Disable crash handling" OFF)
set_option(TRACY_TIMER_FALLBACK "Use lower resolution timers" OFF)
set_option(TRACY_LIBUNWIND_BACKTRACE "Use libunwind backtracing where supported" OFF)
set_option(TRACY_SYMBOL_OFFLINE_RESOLVE "Instead of full runtime symbol resolution, only resolve the image path and offset to enable offline symbol resolution" OFF)
set_option(TRACY_LIBBACKTRACE_ELF_DYNLOAD_SUPPORT "Enable libbacktrace to support dynamically loaded elfs in symbol resolution resolution after the first symbol resolve operation" OFF)

# advanced
set_option(TRACY_VERBOSE "[advanced] Verbose output from the profiler" OFF)
mark_as_advanced(TRACY_VERBOSE)
set_option(TRACY_DEMANGLE "[advanced] Don't use default demangling function - You'll need to provide your own" OFF)
mark_as_advanced(TRACY_DEMANGLE)

if(NOT TRACY_STATIC)
    target_compile_definitions(TracyClient PRIVATE TRACY_EXPORTS)
    target_compile_definitions(TracyClient PUBLIC TRACY_IMPORTS)
endif()

target_compile_definitions(TracyClient PUBLIC -DTRACY_ENABLE=1 -DTRACY_MANUAL_LIFETIME=1 -DTRACY_DELAYED_INIT=1)

include(CMakePackageConfigHelpers)
include(GNUInstallDirs)

set_target_properties(TracyClient PROPERTIES VERSION ${PROJECT_VERSION})

set(tracy_includes
    ${TRACY_PUBLIC_DIR}/tracy/TracyC.h
    ${TRACY_PUBLIC_DIR}/tracy/Tracy.hpp
    ${TRACY_PUBLIC_DIR}/tracy/TracyD3D11.hpp
    ${TRACY_PUBLIC_DIR}/tracy/TracyD3D12.hpp
    ${TRACY_PUBLIC_DIR}/tracy/TracyLua.hpp
    ${TRACY_PUBLIC_DIR}/tracy/TracyOpenCL.hpp
    ${TRACY_PUBLIC_DIR}/tracy/TracyOpenGL.hpp
    ${TRACY_PUBLIC_DIR}/tracy/TracyVulkan.hpp)

set(client_includes
    ${TRACY_PUBLIC_DIR}/client/tracy_concurrentqueue.h
    ${TRACY_PUBLIC_DIR}/client/tracy_rpmalloc.hpp
    ${TRACY_PUBLIC_DIR}/client/tracy_SPSCQueue.h
    ${TRACY_PUBLIC_DIR}/client/TracyKCore.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyArmCpuTable.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyCallstack.h
    ${TRACY_PUBLIC_DIR}/client/TracyCallstack.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyCpuid.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyDebug.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyDxt1.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyFastVector.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyLock.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyProfiler.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyRingBuffer.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyScoped.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyStringHelpers.hpp
    ${TRACY_PUBLIC_DIR}/client/TracySysPower.hpp
    ${TRACY_PUBLIC_DIR}/client/TracySysTime.hpp
    ${TRACY_PUBLIC_DIR}/client/TracySysTrace.hpp
    ${TRACY_PUBLIC_DIR}/client/TracyThread.hpp)

set(common_includes
    ${TRACY_PUBLIC_DIR}/common/tracy_lz4.hpp
    ${TRACY_PUBLIC_DIR}/common/tracy_lz4hc.hpp
    ${TRACY_PUBLIC_DIR}/common/TracyAlign.hpp
    ${TRACY_PUBLIC_DIR}/common/TracyAlloc.hpp
    ${TRACY_PUBLIC_DIR}/common/TracyApi.h
    ${TRACY_PUBLIC_DIR}/common/TracyColor.hpp
    ${TRACY_PUBLIC_DIR}/common/TracyForceInline.hpp
    ${TRACY_PUBLIC_DIR}/common/TracyMutex.hpp
    ${TRACY_PUBLIC_DIR}/common/TracyProtocol.hpp
    ${TRACY_PUBLIC_DIR}/common/TracyQueue.hpp
    ${TRACY_PUBLIC_DIR}/common/TracySocket.hpp
    ${TRACY_PUBLIC_DIR}/common/TracyStackFrames.hpp
    ${TRACY_PUBLIC_DIR}/common/TracySystem.hpp
    ${TRACY_PUBLIC_DIR}/common/TracyUwp.hpp
    ${TRACY_PUBLIC_DIR}/common/TracyYield.hpp)

include(CMakePackageConfigHelpers)
configure_package_config_file(${CMAKE_CURRENT_SOURCE_DIR}/Config.cmake.in
        "${CMAKE_CURRENT_BINARY_DIR}/TracyConfig.cmake"
        INSTALL_DESTINATION ${CMAKE_INSTALL_DATAROOTDIR}/Tracy)