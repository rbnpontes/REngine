#pragma once
#include <rengine/graphics/renderer.h>
#include <rengine/graphics/drawing.h>
#include <rengine/graphics/models.h>
#include <rengine/graphics/shader_manager.h>
#include <rengine/graphics/pipeline_state_manager.h>
#include <rengine/graphics/buffer_manager.h>
#include <rengine/graphics/render_target_manager.h>

namespace rengine {
	namespace graphics {
		R_EXPORT u32 get_msaa_sample_count();
		R_EXPORT u16 get_default_backbuffer_format();
		R_EXPORT u16 get_default_depthbuffer_format();
	}
}