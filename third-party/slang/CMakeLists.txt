include(ExternalProject)

set (SLANG_TAG "2025.12")
set (SLANG_BASE_RELEASE_URL "https://github.com/shader-slang/slang/releases/download/v${SLANG_TAG}/slang-${SLANG_TAG}")
set (SLANG_FILE_SUFFIX "")
set (SLANG_FILE_HASH "")
set (SLANG_BASE_PATH "${CMAKE_BINARY_DIR}/_deps/slang")
set (SLANG_DOWNLOAD_OUTPUT_PATH "${SLANG_BASE_PATH}/slang-pkg.zip")
set (SLANG_SOURCE_PATH "${SLANG_BASE_PATH}/slang-source")

if (WIN32)
  set (SLANG_FILE_SUFFIX "windows-x86_64")
  set (SLANG_FILE_HASH "2adc95e76cbe07c0caf669a373997382772f589bc7f164347a79b429e4c41208")
elseif(LINUX)
  set (SLANG_FILE_SUFFIX "linux-x86_64")
  set (SLANG_FILE_HASH "b6ad38750d521807e56cf51ed66ffc13d6b300aa6610af1ef35d0bee9eb8833b")
elseif(APPLE)
  set (SLANG_FILE_SUFFIX "macos-x86_64")
  set (SLANG_FILE_HASH "d2c9a95a06eadf992f07212d86659802b0c3b9775caff73c322a78e7ef663c6c")
else()
    message(FATAL_ERROR "Unsupported platform")
endif()

set (SLANG_PKG_URL "${SLANG_BASE_RELEASE_URL}-${SLANG_FILE_SUFFIX}.zip")
message(STATUS "Downloading Slang Release: ${SLANG_PKG_URL}")
message(STATUS "Slang Output Path: ${SLANG_DOWNLOAD_OUTPUT_PATH}")

if (NOT EXISTS ${SLANG_BASE_PATH})
  file(MAKE_DIRECTORY ${SLANG_BASE_PATH})

  if(EXISTS ${SLANG_SOURCE_PATH})
    file(REMOVE_RECURSE ${SLANG_SOURCE_PATH})
  endif()
endif()

if (NOT EXISTS ${SLANG_SOURCE_PATH})
  file (MAKE_DIRECTORY ${SLANG_SOURCE_PATH})
endif()

function(setup_slang_pkg)
  if(EXISTS ${SLANG_DOWNLOAD_OUTPUT_PATH})
    file(SHA256 ${SLANG_DOWNLOAD_OUTPUT_PATH} SLANG_DOWNLOADED_HASH)
    if(SLANG_DOWNLOADED_HASH STREQUAL SLANG_FILE_HASH)
      message(STATUS "Slang package already downloaded: ${SLANG_DOWNLOAD_OUTPUT_PATH}. Skipping download process!")
      return()
    else()
      message(STATUS "It seems that Slang package has been corrupted. Redownloading...")
      file(REMOVE ${SLANG_DOWNLOAD_OUTPUT_PATH})
    endif()
  endif()

  file(DOWNLOAD
    ${SLANG_PKG_URL}
    ${SLANG_DOWNLOAD_OUTPUT_PATH}
    SHOW_PROGRESS
    STATUS status
    LOG log
  )

  message(STATUS "Unpacking Slang Package. Package: ${SLANG_DOWNLOAD_OUTPUT_PATH}")
  message(STATUS "Extracting to: ${SLANG_SOURCE_PATH}")
  
  execute_process(
      COMMAND ${CMAKE_COMMAND} -E tar xzf ${SLANG_DOWNLOAD_OUTPUT_PATH}
      WORKING_DIRECTORY ${SLANG_SOURCE_PATH}
  )

  file (GLOB SLANG_HEADERS "${SLANG_SOURCE_PATH}/include/*.h")
  file (MAKE_DIRECTORY ${SLANG_SOURCE_PATH}/include/slang)
  foreach(SLANG_HEADER ${SLANG_HEADERS})
    get_filename_component(SLANG_HEADER_NAME ${SLANG_HEADER} NAME)
    file(RENAME ${SLANG_HEADER} ${SLANG_SOURCE_PATH}/include/slang/${SLANG_HEADER_NAME})
  endforeach()
endfunction()

function (setup_slang)
  file (GLOB SLANG_LIB_FILES "${SLANG_SOURCE_PATH}/lib/*")

  add_library(slang INTERFACE)
  target_include_directories(slang INTERFACE "${SLANG_SOURCE_PATH}/include")
  foreach (SLANG_LIB ${SLANG_LIB_FILES})
    message(STATUS "Slang Lib: ${SLANG_LIB}")
    target_link_libraries(slang INTERFACE "${SLANG_LIB}")
  endforeach ()
endfunction()

setup_slang_pkg()
setup_slang()