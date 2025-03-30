#pragma once
#include "../base_private.h"
#include <GraphicsTypes.h>
#include <DeviceContext.h>

namespace rengine {
    namespace graphics {
        enum class renderer_dirty_flags : u32 {
            none            = 0,
            render_targets  = 1 << 0,
            clear_color     = 1 << 1,
            clear_depth     = 1 << 2,
            clear_stencil   = 1 << 3,
            vertex_buffer   = 1 << 4,
            index_buffer    = 1 << 5,
        };

        struct renderer_state {
            Diligent::ITextureView* render_target[DILIGENT_MAX_RENDER_TARGETS]{};
            Diligent::ITextureView* depth_stencil{ null };
            u8 clear_rt_index{ 0 };
            float clear_color[4] RENDERER_DEFAULT_CLEAR_COLOR;
            float clear_depth_value{ 0 };
            u8 clear_stencil_value{ 0 };
            u8 num_render_targets{ 0 };

            u32 dirty_flags{ (u32)renderer_dirty_flags::none };
        };

        extern renderer_state g_renderer_state;

        void renderer__reset_state();
        void renderer__submit_render_state();
    }
}