#include "./buffer_manager.h"
#include "./buffer_manager_private.h"
#include "./graphics_private.h"

#include "../exceptions.h"
#include "../strings.h"
#include "../io/logger.h"

#include <fmt/format.h>

namespace rengine {
    namespace graphics {
        vertex_buffer_t buffer_mgr_vbuffer_create(const buffer_create_desc& desc)
        {
            return buffer_mgr__try_create(buffer_type::vertex_buffer, desc);
        }

        index_buffer_t buffer_mgr_ibuffer_create(const buffer_create_desc& desc)
        {
            return buffer_mgr__try_create(buffer_type::index_buffer, desc);
        }

        constant_buffer_t buffer_mgr_cbuffer_create(const buffer_create_desc& desc)
        {
            buffer_create_desc desc_copy = desc;
            if (!desc_copy.dynamic)
                io::logger_warn(strings::logs::g_buffer_mgr_tag, strings::logs::g_buffer_mgr_cbuffer_must_be_dyn);
            desc_copy.dynamic = true;
            desc_copy.initial_data = null;
            desc_copy.size = 0;
            return buffer_mgr__try_create(buffer_type::constant_buffer, desc);
        }

        void buffer_mgr_vbuffer_update(const vertex_buffer_t& id, ptr data, u32 size, u32 offset)
        {
			buffer_mgr__update(buffer_type::vertex_buffer, id, data, size, offset);
        }

        void buffer_mgr_ibuffer_update(const index_buffer_t& id, ptr data, u32 size, u32 offset)
        {
			buffer_mgr__update(buffer_type::index_buffer, id, data, size, offset);
        }

        void buffer_mgr_cbuffer_update(const constant_buffer_t& id, ptr data, u32 size, u32 offset)
        {
			buffer_mgr__update(buffer_type::constant_buffer, id, data, size, offset);
        }

        ptr buffer_mgr_vbuffer_map(const vertex_buffer_t& id, const buffer_map_type& type)
        {
            return buffer_mgr__map(buffer_type::vertex_buffer, id, type);
        }

        ptr buffer_mgr_ibuffer_map(const index_buffer_t& id, const buffer_map_type& type)
        {
            return buffer_mgr__map(buffer_type::index_buffer, id, type);
        }

        ptr buffer_mgr_cbuffer_map(const constant_buffer_t& id, const buffer_map_type& type)
        {
            return buffer_mgr__map(buffer_type::constant_buffer, id, type);
        }

        void buffer_mgr_vbuffer_unmap(const vertex_buffer_t& id)
        {
			buffer_mgr__unmap(buffer_type::vertex_buffer, id);
        }

        void buffer_mgr_ibuffer_unmap(const index_buffer_t& id)
        {
            buffer_mgr__unmap(buffer_type::index_buffer, id);
        }

        void buffer_mgr_cbuffer_unmap(const constant_buffer_t& id)
        {
            buffer_mgr__unmap(buffer_type::constant_buffer, id);
        }

        u16 buffer_mgr_vbuffer_realloc(const vertex_buffer_t& buffer_id, u32 new_size)
        {
			return buffer_mgr__realloc(buffer_type::vertex_buffer, buffer_id, new_size);
        }

        u16 buffer_mgr_ibuffer_realloc(const index_buffer_t& id, u32 new_size)
        {
			return buffer_mgr__realloc(buffer_type::index_buffer, id, new_size);
        }

        u16 buffer_mgr_cbuffer_realloc(const constant_buffer_t& id, u32 new_size)
        {
			return buffer_mgr__realloc(buffer_type::constant_buffer, id, new_size);
        }

        void buffer_mgr_vbuffer_free(const vertex_buffer_t& id)
        {
			buffer_mgr__free(buffer_type::vertex_buffer, id);
        }

        void buffer_mgr_ibuffer_free(const index_buffer_t& id)
        {
			buffer_mgr__free(buffer_type::index_buffer, id);
        }

        void buffer_mgr_cbuffer_free(const constant_buffer_t& id)
        {
			buffer_mgr__free(buffer_type::constant_buffer, id);
        }

        bool buffer_mgr_is_valid_vbuffer(const vertex_buffer_t& id)
        {
            return buffer_mgr__is_valid(buffer_type::vertex_buffer, id);
        }

        bool buffer_mgr_is_valid_ibuffer(const index_buffer_t& id)
        {
			return buffer_mgr__is_valid(buffer_type::index_buffer, id);
        }

        bool buffer_mgr_is_valid_cbuffer(const constant_buffer_t& id)
        {
			return buffer_mgr__is_valid(buffer_type::constant_buffer, id);
        }

        ptr buffer_mgr_get_vbuffer_handle(const vertex_buffer_t& id)
        {
            Diligent::IBuffer* buffer;
			buffer_mgr__get_handle(buffer_type::vertex_buffer, id, &buffer);
            return buffer;
        }

        ptr buffer_mgr_get_ibuffer_handle(const index_buffer_t& id)
        {
            Diligent::IBuffer* buffer;
			buffer_mgr__get_handle(buffer_type::index_buffer, id, &buffer);
            return buffer;
        }

        ptr buffer_mgr_get_cbuffer_handle(const constant_buffer_t& id)
        {
            Diligent::IBuffer* buffer;
			buffer_mgr__get_handle(buffer_type::constant_buffer, id, &buffer);
            return buffer;
        }

        u32 buffer_mgr_get_vbuffers_count()
        {
			return buffer_mgr__get_count(buffer_type::vertex_buffer);
        }

        u32 buffer_mgr_get_ibuffers_count()
        {
			return buffer_mgr__get_count(buffer_type::index_buffer);
        }

        u32 buffer_mgr_get_cbuffers_count()
        {
			return buffer_mgr__get_count(buffer_type::constant_buffer);
        }

        u32 buffer_mgr_get_buffers_count()
        {
            return buffer_mgr__get_total_count();
        }

        void buffer_mgr_clear_vbuffers_cache()
        {
			buffer_mgr__clear_cache(buffer_type::vertex_buffer);
        }

        void buffer_mgr_clear_ibuffers_cache()
        {
			buffer_mgr__clear_cache(buffer_type::index_buffer);
        }

        void buffer_mgr_clear_cbuffers_cache()
        {
			buffer_mgr__clear_cache(buffer_type::constant_buffer);
        }

        void buffer_mgr_get_vbuffers_available(u32* count, vertex_buffer_t* buffers)
        {
			buffer_mgr__get_available_buffers(buffer_type::vertex_buffer, count, buffers);
        }

        void buffer_mgr_get_ibuffers_available(u32* count, index_buffer_t* buffers)
        {
			buffer_mgr__get_available_buffers(buffer_type::index_buffer, count, buffers);
        }

        void buffer_mgr_get_cbuffers_available(u32* count, constant_buffer_t* buffers)
        {
			buffer_mgr__get_available_buffers(buffer_type::constant_buffer, count, buffers);
        }
    }
}