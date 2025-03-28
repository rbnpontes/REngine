include(FetchContent)
FetchContent_Declare(
  DiligentCore
  GIT_REPOSITORY https://github.com/DiligentGraphics/DiligentCore.git
  GIT_TAG        b036337d68be2353c9950a85929acf796b9a6d50 # v2.5.6
)
FetchContent_MakeAvailable(DiligentCore)