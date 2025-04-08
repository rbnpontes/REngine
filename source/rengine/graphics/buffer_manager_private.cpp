#include "./buffer_manager_private.h"
#include "./graphics_private.h"

#include "../exceptions.h"
#include "../strings.h"

#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		buffer_mgr_state g_buffer_mgr_state = {};

		void buffer_mgr__init()
		{
			g_buffer_mgr_state.log = io::logger_use(strings::logs::g_buffer_mgr_tag);
		}

		void buffer_mgr__deinit()
		{
			auto& state = g_buffer_mgr_state;
			buffer_entry* buffers[] = {
				state.vertex_buffers.data(),
				state.index_buffers.data(),
				state.constant_buffers.data()
			};
			u32 buffer_sizes[] = {
				GRAPHICS_MAX_ALLOC_VBUFFERS,
				GRAPHICS_MAX_ALLOC_IBUFFERS,
				GRAPHICS_MAX_ALLOC_CBUFFERS
			};

			for (u8 i = 0; i < _countof(buffers); ++i) {
				auto entries = buffers[i];
				const auto& size = buffer_sizes[i];
				for (u32 i = 0; i < size; ++i) {
					const auto& entry = entries[i];
					if (!entry.handler)
						continue;
					entry.handler->Release();
				}
			}

			g_buffer_mgr_state = {};
		}

		u16 buffer_mgr__encode_id(u8 idx, u8 magic)
		{
			return ((u16)idx << 16) | (u16)idx;
		}

		u8 buffer_mgr__decode_id(u16 value)
		{
			return value >> 16;
		}

		u16 buffer_mgr__try_create(const buffer_type& type, const buffer_create_desc& desc)
		{
			auto& state = g_buffer_mgr_state;
			buffer_entry* buffers[] = {
				state.vertex_buffers.data(),
				state.index_buffers.data(),
				state.constant_buffers.data()
			};
			const u32 buffer_sizes[] = {
				GRAPHICS_MAX_ALLOC_VBUFFERS,
				GRAPHICS_MAX_ALLOC_IBUFFERS,
				GRAPHICS_MAX_ALLOC_CBUFFERS
			};
			u8* buffer_counters = reinterpret_cast<u8*>(&state.counts);
			u8* magic_numbers = reinterpret_cast<u8*>(&state.magic_nums);

			u8 buffer_idx = buffer_mgr__find_empty_entry(buffers[(u8)type], buffer_sizes[(u8)type]);
			if (buffer_sizes[(u8)type] == buffer_counters[(u8)type])
				throw graphics_exception(
					fmt::format(strings::exceptions::g_buffer_mgr_reach_limit,
						strings::g_buffer_names[(u8)type],
						buffer_counters[(u8)type]).c_str()
				);

			buffer_entry new_entry = {
				null,
				buffer_mgr__encode_id(buffer_idx, magic_numbers[(u8)type]++)
			};
			++buffer_counters[(u8)type];

			auto hash_suitable_dynamic_buffer = buffer_mgr__find_existent_buffer(type, desc, &buffer_idx);
			// Constant Buffer must not be recycled
			if (hash_suitable_dynamic_buffer && type != buffer_type::constant_buffer) {
				const auto& target_entry = buffers[(u8)type][buffer_idx];
				target_entry.handler->AddRef();

				new_entry.handler = target_entry.handler;
			}
			else {
				new_entry.handler = buffer_mgr__create(type, desc);
			}

			buffers[(u8)type][buffer_idx] = new_entry;
			return new_entry.id;
		}

		void buffer_mgr__free(const buffer_type& type, u16 buffer_id)
		{
			const auto log = g_buffer_mgr_state.log;
			if (!buffer_mgr__is_valid(type, buffer_id)) {
				log->warn(
					fmt::format(strings::logs::g_buffer_mgr_free_invalid_buffer,
						buffer_id).c_str()
				);
				return;
			}

			const auto& idx = buffer_mgr__decode_id(buffer_id);
			buffer_entry* buffer_entries[] = {
				g_buffer_mgr_state.vertex_buffers.data(),
				g_buffer_mgr_state.index_buffers.data(),
				g_buffer_mgr_state.constant_buffers.data()
			};
			const auto ctx = g_graphics_state.contexts[0];
			auto& buffer_entry = buffer_entries[(u8)type][idx];
			const auto handle = buffer_entry.handler;

			handle->Release();
			buffer_entry.handler = null;
			buffer_entry.id = 0;

			u8* buffer_counts = reinterpret_cast<u8*>(&g_buffer_mgr_state.counts);
			--buffer_counts[(u8)type];
		}

		void buffer_mgr__realloc(const buffer_type& type, u16 buffer_id, u32 new_size)
		{
			const auto& idx = buffer_mgr__assert_id(type, buffer_id);
			buffer_entry* buffer_entries[] = {
				g_buffer_mgr_state.vertex_buffers.data(),
				g_buffer_mgr_state.index_buffers.data(),
				g_buffer_mgr_state.constant_buffers.data()
			};
			const auto ctx = g_graphics_state.contexts[0];
			auto& buffer_entry = buffer_entries[(u8)type][idx];
			const auto handle = buffer_entry.handler;
			const auto desc = handle->GetDesc();

			if (desc.Usage != Diligent::USAGE_DYNAMIC)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_buffer_mgr_cant_realloc_non_dyn,
						buffer_id,
						desc.Name,
						strings::g_buffer_names[(u8)type]).c_str()
				);

			handle->Release();

			buffer_entry.handler = buffer_mgr__create(type, {
				desc.Name,
				new_size,
				null,
				true
			});
		}

		void buffer_mgr__update(const buffer_type& type, u16 buffer_id, ptr data, u32 size)
		{
			const auto& idx = buffer_mgr__assert_id(type, buffer_id);
			const buffer_entry* buffer_entries[] = {
				g_buffer_mgr_state.vertex_buffers.data(),
				g_buffer_mgr_state.index_buffers.data(),
				g_buffer_mgr_state.constant_buffers.data()
			};

			const auto log = g_buffer_mgr_state.log;
			const auto ctx = g_graphics_state.contexts[0];
			const auto& buffer_entry = buffer_entries[(u8)type][idx];
			const auto handle = buffer_entry.handler;
			const auto& desc = handle->GetDesc();
			if (desc.Usage != Diligent::USAGE_DYNAMIC) {
				log->error(
					fmt::format(strings::logs::g_buffer_mgr_cant_update_non_dyn,
						buffer_id,
						desc.Name,
						strings::g_buffer_names[(u8)type]).c_str());
				return;
			}

			if (size > desc.Size) {
				log->warn(
					fmt::format(strings::logs::g_buffer_mgr_update_data_size_is_greater_than_buffer,
						size,
						buffer_id,
						desc.Name,
						desc.Size,
						strings::g_buffer_names[(u8)type]).c_str()
				);
				size = desc.Size;
			}

			ptr mapped_data = null;
			ctx->MapBuffer(handle, Diligent::MAP_WRITE, Diligent::MAP_FLAG_DISCARD, mapped_data);
			if (mapped_data == null)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_buffer_mgr_failed_to_update_buffer,
						buffer_id,
						desc.Name,
						strings::g_buffer_names[(u8)type]).c_str()
				);

			memcpy(mapped_data, data, size);
			ctx->UnmapBuffer(handle, Diligent::MAP_WRITE);
		}

		Diligent::IBuffer* buffer_mgr__create(const buffer_type& type, const buffer_create_desc& desc)
		{
			const auto device = g_graphics_state.device;

			using namespace Diligent;
			BufferDesc buffer_desc = {};
			buffer_desc.Name = desc.name;
			buffer_desc.Size = desc.size;
			buffer_desc.BindFlags = g_bind_flags_tbl[(u8)type];
			buffer_desc.Usage = USAGE_IMMUTABLE;
			buffer_desc.CPUAccessFlags = CPU_ACCESS_NONE;

			if (desc.dynamic) {
				buffer_desc.Usage = USAGE_DYNAMIC;
				buffer_desc.CPUAccessFlags = CPU_ACCESS_WRITE;
			}
			else if (!desc.initial_data)
				throw graphics_exception(strings::exceptions::g_buffer_mgr_requires_initial_data);

			BufferData data = {};
			data.DataSize = desc.size;
			data.pData = desc.initial_data;

			IBuffer* buffer = null;
			device->CreateBuffer(buffer_desc,
				desc.dynamic ? null : &data,
				&buffer);

			if (!buffer)
				throw graphics_exception(
					fmt::format(
						strings::exceptions::g_buffer_mgr_fail_to_create_buffer, 
						strings::g_buffer_names[(u8)type]
					).c_str()
				);
			
			buffer->AddRef();
			return buffer;
		}

		bool buffer_mgr__is_valid(const buffer_type& type, u16 buffer_id)
		{
			const u16 invalid_ids[] = {
				no_vertex_buffer,
				no_index_buffer,
				no_constant_buffer,
			};

			const buffer_entry* buffer_entries[] = {
			   g_buffer_mgr_state.vertex_buffers.data(),
			   g_buffer_mgr_state.index_buffers.data(),
			   g_buffer_mgr_state.constant_buffers.data()
			};
			const u32 buffer_sizes[] = {
				GRAPHICS_MAX_ALLOC_VBUFFERS,
				GRAPHICS_MAX_ALLOC_IBUFFERS,
				GRAPHICS_MAX_ALLOC_CBUFFERS
			};
			const auto type_idx = (u8)type;
			if (invalid_ids[buffer_id] == buffer_id)
				return false;

			const auto& idx = buffer_mgr__decode_id(buffer_id);
			if (idx >= buffer_sizes[type_idx])
				return false;

			const auto& entry = buffer_entries[type_idx][idx];
			return entry.handler != null && entry.id == buffer_id;
		}

		ptr buffer_mgr__get_handle(const buffer_type& type, u16 buffer_id)
		{
			return ptr();
		}

		u32 buffer_mgr__get_count(const buffer_type& type)
		{
			const auto& state = g_buffer_mgr_state;
			const u8* counters = reinterpret_cast<const u8*>(&state.counts);
			return counters[(u8)type];
		}

		u32 buffer_mgr__get_total_count()
		{
			const auto& counts = g_buffer_mgr_state.counts;
			return counts.vbuffers_count + counts.ibuffers_count + counts.cbuffers_count;
		}

		void buffer_mgr__clear_cache(const buffer_type& type)
		{
			buffer_entry* buffers[] = {
				g_buffer_mgr_state.vertex_buffers.data(),
				g_buffer_mgr_state.index_buffers.data(),
				g_buffer_mgr_state.constant_buffers.data()
			};
			u32 buffer_sizes[] = {
				GRAPHICS_MAX_ALLOC_VBUFFERS,
				GRAPHICS_MAX_ALLOC_IBUFFERS,
				GRAPHICS_MAX_ALLOC_CBUFFERS
			};
			u8* magics = reinterpret_cast<u8*>(&g_buffer_mgr_state.magic_nums);
			u8* counters = reinterpret_cast<u8*>(&g_buffer_mgr_state.counts);

			const auto type_idx = (u8)type;
			for (u32 i = 0; i < buffer_sizes[type_idx]; ++i) {
				auto& entry = buffers[type_idx][i];
				if (!entry.handler)
					continue;

				entry.handler->Release();
				entry.handler = null;
				entry.id = 0;
			}

			counters[type_idx] = magics[type_idx] = 0;
		}

		void buffer_mgr__get_available_buffers(const buffer_type& type, u32* count, u16* buffers)
		{
			const auto& state = g_buffer_mgr_state;
			const buffer_entry* buffer_entries[] = {
				state.vertex_buffers.data(),
				state.index_buffers.data(),
				state.constant_buffers.data()
			};
			const u32 buffer_sizes[] = {
				GRAPHICS_MAX_ALLOC_VBUFFERS,
				GRAPHICS_MAX_ALLOC_IBUFFERS,
				GRAPHICS_MAX_ALLOC_CBUFFERS
			};
			const auto type_idx = (u8)type;
			u32 available_count = 0;

			for (u32 i = 0; i < buffer_sizes[type_idx]; ++i) {
				const auto& entry = buffer_entries[type_idx][i];
				if (!entry.handler)
					continue;
				
				if (buffers)
					buffers[available_count] = entry.id;
				++available_count;
			}
			*count = available_count;
		}

		bool buffer_mgr__find_existent_buffer(const buffer_type& type, const buffer_create_desc& desc, u8* output_idx)
		{
			const auto& state = g_buffer_mgr_state;
			const buffer_entry* buffers[] = {
				state.vertex_buffers.data(),
				state.index_buffers.data(),
				state.constant_buffers.data()
			};
			const u32 buffer_sizes[] = {
				GRAPHICS_MAX_ALLOC_VBUFFERS,
				GRAPHICS_MAX_ALLOC_IBUFFERS,
				GRAPHICS_MAX_ALLOC_CBUFFERS
			};
			const auto expected_size = desc.size;

			if (!desc.dynamic)
				return false;

			return buffer_mgr__find_dynamic_buffer(buffers[(u8)type], buffer_sizes[(u8)type], expected_size, output_idx);
		}

		bool buffer_mgr__find_dynamic_buffer(const buffer_entry* buffers, u32 buffers_count, u32 expected_size, u8* output_idx)
		{
			for (size_t i = 0; i < buffers_count; ++i) {
				Diligent::IBuffer* buffer = buffers[i].handler;
				if (!buffer)
					continue;
				const auto& buffer_desc = buffer->GetDesc();
				if (buffer_desc.Usage != Diligent::USAGE_DYNAMIC || expected_size > buffer_desc.Size)
					continue;

				*output_idx = i;
				return true;
			}
			return false;
		}

		u8 buffer_mgr__find_empty_entry(const buffer_entry* buffers, u32 buffers_count)
		{
			for (u32 i = 0; i < buffers_count; ++i) {
				const auto& entry = buffers[i];
				if (entry.handler != null)
					continue;
				return i;
			}

			return MAX_U8_VALUE;
		}
		
		u8 buffer_mgr__assert_id(const buffer_type& type, u16 id) {
			if (buffer_mgr__is_valid(type, id))
				return buffer_mgr__decode_id(id);

			const u32 buffer_sizes[] = {
				GRAPHICS_MAX_ALLOC_VBUFFERS,
				GRAPHICS_MAX_ALLOC_IBUFFERS,
				GRAPHICS_MAX_ALLOC_CBUFFERS
			};
			throw graphics_exception(
				fmt::format(strings::exceptions::g_buffer_mgr_invalid_id,
					buffer_sizes[(u8)type]).c_str()
			);
		}
	}
}