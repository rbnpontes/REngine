#pragma once
#include <float.h>

#define CORE_ALLOC_DEFAULT_LIMIT 128 * 1000000 // default size is 128mb
#define CORE_ALLOC_SCRATCH_BUFFER_SIZE 1000 * 64 // 64kb scratch buffer size
//#define HIGH_DEFINITION_PRECISION // enable high precision math types
#define CORE_WINDOWS_MAX_ALLOWED 4
#define CORE_DEFAULT_HASH_SEED 0xFABDDFE
#define CORE_HASH_PRIME 4094394974U
#define CORE_MAX_PROFILER_ENTRIES 255 // Increate this number if you need more profiler entries

#define IO_MAX_LOG_OBJECTS 0xFF

// Max number of scissors allowed per render command
#define GRAPHICS_MAX_SCISSORS 4

#define GRAPHICS_MAX_RENDER_TARGETS 4
#define GRAPHICS_MAX_VBUFFERS 2
#define GRAPHICS_MAX_RENDER_COMMANDS 0xFF
#define GRAPHICS_MAX_ALLOC_VBUFFERS 0xFF
#define GRAPHICS_MAX_ALLOC_IBUFFERS 0xFF
#define GRAPHICS_MAX_ALLOC_CBUFFERS 8
#define GRAPHICS_MAX_ALLOC_RENDER_TARGETS 16
#define GRAPHICS_MAX_ALLOC_TEX2D 0xFF
#define GRAPHICS_MAX_ALLOC_TEX3D 1
#define GRAPHICS_MAX_ALLOC_TEXCUBE 2
#define GRAPHICS_MAX_ALLOC_TEXARRAY 1

#define DRAWING_DEFAULT_TRIANGLE_COUNT 16
#define DRAWING_DEFAULT_LINES_COUNT 10
#define DRAWING_DEFAULT_POINTS_COUNT 3
#define DRAWING_MAX_TEXT_LENGTH 256

#define IMGUI_MANAGER_VBUFFER_EXTRA_SIZE 5000 // extra size for ImGui vertex buffer, to avoid reallocations (same as in ImGui samples)
#define IMGUI_MANAGER_IBUFFER_EXTRA_SIZE 10000 // extra size for ImGui index buffer, to avoid reallocations (same as in ImGui samples)

#define LIGHT_ENTITY_SIZE u16

#define MAX_U8_VALUE 0xFF
#define MAX_U16_VALUE 0xFFFF
#define MAX_U32_VALUE 0xFFFFFFFF
#define MAX_FLOAT_VALUE FLT_MAX
#define MAX_DOUBLE_VALUE DBL_MAX

#ifdef HIGH_DEFINITION_PRECISION
	#define MATH_EPSILON 1e-6f
#else
	#define MATH_EPSILON 1e-5f
#endif

#ifdef PLATFORM_WINDOWS
	#define GRAPHICS_BACKEND_DEFAULT rengine::graphics::backend::d3d11
#elif PLATFORM_LINUX
	#define GRAPHICS_BACKEND_DEFAULT rengine::graphics::backend::vulkan
#elif PLATFORM_WEB
	#define GRAPHICS_BACKEND_DEFAULT rengine::graphics::backend::opengl
#else
	#error "Invalid Platform Definition"
#endif

#define RENDERER_DEFAULT_CLEAR_COLOR { 0.354f, 0.354f, 0.354f, 1.0f }

#if CORE_WINDOWS_MAX_ALLOWED < 1
	#error "MAX_ALLOWED_WINDOWS must be greater than 0"
#endif
#if CORE_WINDOWS_MAX_ALLOWED > 254
	#error "MAX_ALLOWED_WINDOWS must be less than 254"
#endif

#if GRAPHICS_MAX_ALLOC_VBUFFERS > 0xFF
	#error "GRAPHICS_MAX_ALLOC_VBUFFERS must be less than 255, or you can increase vertex buffer size to u32"
#endif
#if GRAPHICS_MAX_ALLOC_IBUFFERS > 0xFF
	#error "GRAPHICS_MAX_ALLOC_IBUFFERS must be less than 255, or you can increase index buffer size to u32"
#endif
#if GRAPHICS_MAX_ALLOC_CBUFFERS > 0xFF
	#error "GRAPHICS_MAX_ALLOC_CBUFFERS must be less than 255, or you can increase constant buffer size to u32"
	#error "Do you really need more than 255 allocated constant buffers ?"
#endif

#if _DEBUG
#define ENGINE_DEBUG 1
#endif

#define ENGINE_SSE 1
#define ENGINE_PROFILER 1

#define nameof(name) (#name)