#pragma once
#include <rengine/api.h>
#include <rengine/types.h>

namespace rengine {
    namespace graphics {
        enum class buffer_type {
            vertex_buffer = 0,
            index_buffer,
            constant_buffer
        };

        struct buffer_create_desc {
            c_str name{ null };
            u32 size{ 0 };
            ptr initial_data{ null };
            bool dynamic{ false };
        };

        struct buffer_update_desc {
            buffer_type type;
            u32 id;
            ptr source;
            u32 size;
        };

        vertex_buffer_t buffer_mgr_vbuffer_create(const buffer_create_desc& desc);
        index_buffer_t buffer_mgr_ibuffer_create(const buffer_create_desc& desc);
        constant_buffer_t buffer_mgr_cbuffer_create(const buffer_create_desc& desc);

		R_EXPORT void buffer_mgr_vbuffer_update(const vertex_buffer_t& id, ptr data, u32 size);
		R_EXPORT void buffer_mgr_ibuffer_update(const index_buffer_t& id, ptr data, u32 size);
		R_EXPORT void buffer_mgr_cbuffer_update(const constant_buffer_t& id, ptr data, u32 size);

		R_EXPORT u16 buffer_mgr_vbuffer_realloc(const vertex_buffer_t& buffer_id, u32 new_size);
		R_EXPORT u16 buffer_mgr_ibuffer_realloc(const index_buffer_t& id, u32 new_size);
		R_EXPORT u16 buffer_mgr_cbuffer_realloc(const constant_buffer_t& id, u32 new_size);

		R_EXPORT void buffer_mgr_vbuffer_free(const vertex_buffer_t& id);
        R_EXPORT void buffer_mgr_ibuffer_free(const index_buffer_t& id);
		R_EXPORT void buffer_mgr_cbuffer_free(const constant_buffer_t& id);

		R_EXPORT bool buffer_mgr_is_valid_vbuffer(const vertex_buffer_t& id);
		R_EXPORT bool buffer_mgr_is_valid_ibuffer(const index_buffer_t& id);
		R_EXPORT bool buffer_mgr_is_valid_cbuffer(const constant_buffer_t& id);

		R_EXPORT ptr buffer_mgr_get_vbuffer_handle(const vertex_buffer_t& id);
		R_EXPORT ptr buffer_mgr_get_ibuffer_handle(const index_buffer_t& id);
		R_EXPORT ptr buffer_mgr_get_cbuffer_handle(const constant_buffer_t& id);

		R_EXPORT u32 buffer_mgr_get_vbuffers_count();
        R_EXPORT u32 buffer_mgr_get_ibuffers_count();
		R_EXPORT u32 buffer_mgr_get_cbuffers_count();
        R_EXPORT u32 buffer_mgr_get_buffers_count();

        R_EXPORT void buffer_mgr_clear_vbuffers_cache();
		R_EXPORT void buffer_mgr_clear_ibuffers_cache();
		R_EXPORT void buffer_mgr_clear_cbuffers_cache();

		R_EXPORT void buffer_mgr_get_vbuffers_available(u32* count, vertex_buffer_t* buffers);
		R_EXPORT void buffer_mgr_get_ibuffers_available(u32* count, index_buffer_t* buffers);
		R_EXPORT void buffer_mgr_get_cbuffers_available(u32* count, constant_buffer_t* buffers);
    }
}