#pragma once
#include "../base_private.h"
#include <GraphicsTypes.h>
#include <DeviceContext.h>

namespace rengine {
    namespace graphics {
        enum class renderer_dirty_flags : u32 {
            none            = 0,
            render_targets  = 1 << 0,
            vertex_buffer   = 1 << 1,
            index_buffer    = 1 << 2,
        };

        struct renderer_state {
            Diligent::ITextureView* render_targets[DILIGENT_MAX_RENDER_TARGETS]{};
            Diligent::ITextureView* depth_stencil{ null };
            u32 dirty_flags { (u32)renderer_dirty_flags::none };
            u8 num_render_targets{ 0 };
        };

        extern renderer_state g_renderer_state;

        void renderer__reset_state();
        void renderer__set_render_targets();
        void renderer__submit_render_state();
        void renderer__assert_render_target_idx(u8 idx);
    }
}