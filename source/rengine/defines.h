#pragma once
#define CORE_ALLOC_DEFAULT_LIMIT 128 * 1000000 // default size is 128mb
//#define HIGH_DEFINITION_PRECISION // enable high precision math types
#define CORE_WINDOWS_MAX_ALLOWED 4
#define CORE_DEFAULT_HASH_SEED 0xFABDDFE
#define CORE_HASH_PRIME 4094394974U

#define IO_MAX_LOG_OBJECTS 0xFF

#define GRAPHICS_MAX_RENDER_TARGETS 4
#define GRAPHICS_MAX_VBUFFERS 2
#define GRAPHICS_MAX_RENDER_COMMANDS 0xFF
#define GRAPHICS_MAX_ALLOC_VBUFFERS 0xFF
#define GRAPHICS_MAX_ALLOC_IBUFFERS 0xFF
#define GRAPHICS_MAX_ALLOC_CBUFFERS 8
#define GRAPHICS_MAX_ALLOC_RENDER_TARGETS 16

#define DRAWING_DEFAULT_TRIANGLE_COUNT 16
#define DRAWING_DEFAULT_LINES_COUNT 10
#define DRAWING_DEFAULT_POINTS_COUNT 3

#define LIGHT_ENTITY_SIZE u16

#define MAX_U8_VALUE 0xFF
#define MAX_U16_VALUE 0xFFFF
#define MAX_U32_VALUE 0xFFFFFFFF

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