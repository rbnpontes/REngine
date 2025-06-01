#pragma once
#include <rengine/graphics/renderer.h>
#include <rengine/graphics/drawing.h>
#include <rengine/graphics/models.h>
#include <rengine/graphics/shader_manager.h>
#include <rengine/graphics/pipeline_state_manager.h>
#include <rengine/graphics/buffer_manager.h>
#include <rengine/graphics/render_target_manager.h>
#include <rengine/graphics/texture_manager.h>
#include <rengine/graphics/imgui_manager.h>

namespace rengine {
	namespace graphics {
		R_EXPORT void enable_vsync();
		R_EXPORT void disable_vsync();
		R_EXPORT bool vsync_enabled();
		R_EXPORT void set_msaa_level(u8 lvl);
		R_EXPORT u8 get_msaa_level();
		R_EXPORT u8 get_msaa_available_levels();
		R_EXPORT u16 get_default_backbuffer_format();
		R_EXPORT u16 get_default_depthbuffer_format();
	}
}