#pragma once
#include "../base_private.h"
#include "./buffer_manager.h"
#include "../io/logger.h"

#include <GraphicsTypes.h>
#include <Buffer.h>

namespace rengine {
    namespace graphics {
        struct buffer_mgr_magic_nums {
            u8 vbuffer{ 0 };
            u8 ibuffer{ 0 };
            u8 cbuffer{ 0 };
        };

        struct buffer_mgr_counts {
            u8 vbuffers_count{ 0 };
            u8 ibuffers_count{ 0 };
            u8 cbuffers_count{ 0 };
        };

        struct buffer_entry {
            Diligent::IBuffer* handler{ null };
            u16 id { 0 };
        };

        struct buffer_mgr_state {
            array<buffer_entry, GRAPHICS_MAX_ALLOC_VBUFFERS> vertex_buffers{};
            array<buffer_entry, GRAPHICS_MAX_ALLOC_IBUFFERS> index_buffers{};
            array<buffer_entry, GRAPHICS_MAX_ALLOC_CBUFFERS> constant_buffers{};

            buffer_mgr_magic_nums magic_nums{};
            buffer_mgr_counts counts{};
            io::ILog* log{ null };
        };

        static constexpr Diligent::BIND_FLAGS g_bind_flags_tbl[] = {
            Diligent::BIND_VERTEX_BUFFER,
            Diligent::BIND_INDEX_BUFFER,
            Diligent::BIND_SHADER_RESOURCE
        };
        extern buffer_mgr_state g_buffer_mgr_state;

        void buffer_mgr__init();
        void buffer_mgr__deinit();

        u16 buffer_mgr__encode_id(u8 idx, u8 magic);
        u8 buffer_mgr__decode_id(u16 value);
        
        u16 buffer_mgr__try_create(const buffer_type& type, const buffer_create_desc& desc);
		void buffer_mgr__free(const buffer_type& type, u16 buffer_id);
        u16 buffer_mgr__realloc(const buffer_type& type, u16 buffer_id, u32 new_size);
		void buffer_mgr__update(const buffer_type& type, u16 buffer_id, ptr data, u32 size);
        Diligent::IBuffer* buffer_mgr__create(const buffer_type& type, const buffer_create_desc& desc);
		bool buffer_mgr__is_valid(const buffer_type& type, u16 buffer_id);
        void buffer_mgr__get_handle(const buffer_type& type, u16 buffer_id, Diligent::IBuffer** output);
		u32 buffer_mgr__get_count(const buffer_type& type);
        u32 buffer_mgr__get_total_count();
		void buffer_mgr__clear_cache(const buffer_type& type);
		void buffer_mgr__get_available_buffers(const buffer_type& type, u32* count, u16* buffers);

        bool buffer_mgr__find_existent_buffer(const buffer_type& type, const buffer_create_desc& desc, u8* output_idx);
        bool buffer_mgr__find_dynamic_buffer(const buffer_entry* buffers, u32 buffers_count, u32 expected_size, u8* output_idx);
        u8 buffer_mgr__find_empty_entry(const buffer_entry* buffers, u32 buffers_count);
        u8 buffer_mgr__assert_id(const buffer_type& type, u16 id);
        buffer_entry* buffer_mgr__get_entry(const buffer_type& type, u16 id);
    }
}