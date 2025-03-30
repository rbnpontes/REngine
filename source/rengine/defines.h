#pragma once
#define ALLOC_DEFAULT_LIMIT 128 * 1000000 // default size is 128mb
//#define HIGH_DEFINITION_PRECISION // enable high precision math types
#define MAX_ALLOWED_WINDOWS 4

#define MAX_U8_VALUE 0xFF
#define MAX_U32_VALUE 0xFFFFFFFF

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

#if MAX_ALLOWED_WINDOWS < 1
	#error "MAX_ALLOWED_WINDOWS must be greater than 0"
#endif
#if MAX_ALLOWED_WINDOWS > 254
	#error "MAX_ALLOWED_WINDOWS must be less than 254"
#endif