#pragma once
#include "../base_private.h"
#include "./buffer_manager.h"
#include "../io/logger.h"
#include "../core/pool.h"

#include <GraphicsTypes.h>
#include <Buffer.h>

namespace rengine {
    namespace graphics {
        struct buffer_entry {
            Diligent::IBuffer* handler{ null };
            buffer_map_type map_type{ buffer_map_type::none };
        };

        struct buffer_mgr_state {
            core::array_pool<buffer_entry, GRAPHICS_MAX_ALLOC_VBUFFERS> vertex_buffers{};
            core::array_pool<buffer_entry, GRAPHICS_MAX_ALLOC_IBUFFERS> index_buffers{};
            core::array_pool<buffer_entry, GRAPHICS_MAX_ALLOC_CBUFFERS> constant_buffers{};

            vertex_buffer_t dynamic_vbuffer{ no_vertex_buffer };
            index_buffer_t dynamic_ibuffer{ no_index_buffer };

            io::ILog* log{ null };
        };

		static constexpr Diligent::MAP_TYPE g_map_type_tbl[] = {
            Diligent::MAP_READ,
			Diligent::MAP_READ,
			Diligent::MAP_WRITE,
			Diligent::MAP_READ_WRITE
		};
        static constexpr Diligent::BIND_FLAGS g_bind_flags_tbl[] = {
            Diligent::BIND_VERTEX_BUFFER,
            Diligent::BIND_INDEX_BUFFER,
            Diligent::BIND_UNIFORM_BUFFER
        };
        extern buffer_mgr_state g_buffer_mgr_state;

        void buffer_mgr__init();
        void buffer_mgr__deinit();
        void buffer_mgr__free_buffer(const buffer_entry& entry);

        u16 buffer_mgr__encode_id(u8 idx, u8 magic);
        u8 buffer_mgr__decode_id(u16 value);
        
        u16 buffer_mgr__try_create(const buffer_type& type, const buffer_create_desc& desc);
		void buffer_mgr__free(const buffer_type& type, u16 buffer_id);
        u16 buffer_mgr__realloc(const buffer_type& type, u16 buffer_id, u32 new_size);
		void buffer_mgr__update(const buffer_type& type, u16 buffer_id, ptr data, u32 size, u32 offset);
		ptr buffer_mgr__map(const buffer_type& type, u16 buffer_id, const buffer_map_type& map_type);
        void buffer_mgr__unmap(const buffer_type& type, u16 buffer_id);
        Diligent::IBuffer* buffer_mgr__create(const buffer_type& type, const buffer_create_desc& desc);
		bool buffer_mgr__is_valid(const buffer_type& type, u16 buffer_id);
        void buffer_mgr__get_handle(const buffer_type& type, u16 buffer_id, Diligent::IBuffer** output);
		u32 buffer_mgr__get_count(const buffer_type& type);
        u32 buffer_mgr__get_total_count();
		void buffer_mgr__clear_cache(const buffer_type& type);
		void buffer_mgr__get_available_buffers(const buffer_type& type, u32* count, u16* buffers);

        void buffer_mgr__get_entry(const buffer_type& type, u16 id, buffer_entry* output);
    }
}