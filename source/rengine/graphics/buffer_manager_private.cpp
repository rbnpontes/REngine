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

			for(const auto& buffer : state.vertex_buffers)
				buffer_mgr__free_buffer(buffer.value);
			for (const auto& buffer : state.index_buffers)
				buffer_mgr__free_buffer(buffer.value);
			for (const auto& buffer : state.constant_buffers)
				buffer_mgr__free_buffer(buffer.value);

			state.vertex_buffers.clear();
			state.index_buffers.clear();
			state.constant_buffers.clear();
			g_buffer_mgr_state = {};
		}

		void buffer_mgr__free_buffer(const buffer_entry& entry)
		{
			auto ctx = g_graphics_state.contexts[0];
			if (entry.map_type != buffer_map_type::none)
				ctx->UnmapBuffer(entry.handler, g_map_type_tbl[(u32)entry.map_type]);
			entry.handler->Release();
		}

		u16 buffer_mgr__encode_id(u8 idx, u8 magic)
		{
			return (idx << 8u) | idx;
		}

		u8 buffer_mgr__decode_id(u16 value)
		{
			return value >> 8u;
		}

		u16 buffer_mgr__try_create(const buffer_type& type, const buffer_create_desc& desc)
		{
			auto& state = g_buffer_mgr_state;
			u16 buffer_id = 0;
			buffer_entry new_entry = {
				buffer_mgr__create(type, desc),
				buffer_map_type::none
			};

			switch (type)
			{
			case buffer_type::vertex_buffer:
				buffer_id = state.vertex_buffers.push_back(new_entry);
				break;
			case buffer_type::index_buffer:
				buffer_id = state.index_buffers.push_back(new_entry);
				break;
			case buffer_type::constant_buffer:
				buffer_id = state.constant_buffers.push_back(new_entry);
				break;
			}

			return buffer_id;
		}

		void buffer_mgr__free(const buffer_type& type, u16 buffer_id)
		{
			const auto& state = g_buffer_mgr_state;
			const auto log = state.log;
			if (!buffer_mgr__is_valid(type, buffer_id)) {
				log->warn(
					fmt::format(strings::logs::g_buffer_mgr_free_invalid_buffer,
						buffer_id).c_str()
				);
				return;
			}

			buffer_entry entry;
			buffer_mgr__get_entry(type, buffer_id, &entry);
			buffer_mgr__free_buffer(entry);
		}

		u16 buffer_mgr__realloc(const buffer_type& type, u16 buffer_id, u32 new_size)
		{
			auto& state = g_buffer_mgr_state;

			buffer_entry entry;
			buffer_mgr__get_entry(type, buffer_id, &entry);
			
			const auto& desc = entry.handler->GetDesc();
			if (desc.Usage != Diligent::USAGE_DYNAMIC)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_buffer_mgr_cant_realloc_non_dyn,
						buffer_id,
						desc.Name,
						strings::g_buffer_names[(u8)type]).c_str()
				);


			buffer_entry new_entry = {
				buffer_mgr__create(type, {
					desc.Name,
					new_size,
					null,
					true
				}),
				buffer_map_type::none
			};

			buffer_mgr__free_buffer(entry);

			switch (type)
			{
			case buffer_type::vertex_buffer:
				buffer_id = state.vertex_buffers.replace(buffer_id, new_entry);
				break;
			case buffer_type::index_buffer:
				buffer_id = state.index_buffers.replace(buffer_id, new_entry);
				break;
			case buffer_type::constant_buffer:
				buffer_id = state.constant_buffers.replace(buffer_id, new_entry);
				break;
			}

			return buffer_id;
		}

		void buffer_mgr__update(const buffer_type& type, u16 buffer_id, ptr data, u32 size, u32 offset)
		{
			const auto& state = g_buffer_mgr_state;
			const auto log = state.log;
			const auto ctx = g_graphics_state.contexts[0];

			buffer_entry entry;
			buffer_mgr__get_entry(type, buffer_id, &entry);	

			const auto& desc = entry.handler->GetDesc();
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

			ptr mapped_data = buffer_mgr__map(type, buffer_id, buffer_map_type::write);
			memcpy(static_cast<u8*>(mapped_data) + offset, data, size);
			buffer_mgr__unmap(type, buffer_id);
		}

		ptr buffer_mgr__map(const buffer_type& type, u16 buffer_id, const buffer_map_type& map_type)
		{
			if (map_type == buffer_map_type::none)
				return null;

			auto& state = g_buffer_mgr_state;
			auto ctx = g_graphics_state.contexts[0];
			ptr mapped_data = null;

			buffer_entry entry;
			buffer_mgr__get_entry(type, buffer_id, &entry);

			ctx->MapBuffer(entry.handler, g_map_type_tbl[(u32)map_type], Diligent::MAP_FLAG_DISCARD, mapped_data);
			if (mapped_data == null)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_buffer_mgr_failed_to_update_buffer,
						buffer_id,
						entry.handler->GetDesc().Name,
						strings::g_buffer_names[(u8)type]).c_str()
				);

			entry.map_type = map_type;
			switch (type)
			{
			case buffer_type::vertex_buffer:
				state.vertex_buffers.overwrite(buffer_id, entry);
				break;
			case buffer_type::index_buffer:
				state.index_buffers.overwrite(buffer_id, entry);
				break;
			case buffer_type::constant_buffer:
				state.constant_buffers.overwrite(buffer_id, entry);
				break;
			}

			return mapped_data;
		}

		void buffer_mgr__unmap(const buffer_type& type, u16 buffer_id)
		{
			auto& state = g_buffer_mgr_state;
			const auto log = state.log;
			auto ctx = g_graphics_state.contexts[0];

			buffer_entry entry;
			buffer_mgr__get_entry(type, buffer_id, &entry);

			if (entry.map_type == buffer_map_type::none) {
				log->warn(
					fmt::format(strings::logs::g_buffer_mgr_cant_unmap,
						buffer_id,
						strings::g_buffer_names[(u8)type]).c_str()
				);
				return;
			}

			ctx->UnmapBuffer(entry.handler, g_map_type_tbl[(u32)entry.map_type]);
			entry.map_type = buffer_map_type::none;

			switch (type)
			{
			case buffer_type::vertex_buffer:
				state.vertex_buffers.overwrite(buffer_id, entry);
				break;
			case buffer_type::index_buffer:
				state.index_buffers.overwrite(buffer_id, entry);
				break;
			case buffer_type::constant_buffer:
				state.constant_buffers.overwrite(buffer_id, entry);
				break;
			}
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
			
			auto* initial_data = &data;

			if (desc.dynamic)
				initial_data = null;
			if (type == buffer_type::constant_buffer)
				initial_data = null;

			IBuffer* buffer = null;
			device->CreateBuffer(buffer_desc,
				initial_data,
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
			bool result = false;
			switch (type)
			{
			case buffer_type::vertex_buffer:
				result = g_buffer_mgr_state.vertex_buffers.is_valid(buffer_id);
				break;
			case buffer_type::index_buffer:
				result = g_buffer_mgr_state.index_buffers.is_valid(buffer_id);
				break;
			case buffer_type::constant_buffer:
				result = g_buffer_mgr_state.constant_buffers.is_valid(buffer_id);
				break;
			}

			return result;
		}

		void buffer_mgr__get_handle(const buffer_type& type, u16 buffer_id, Diligent::IBuffer** output)
		{
			if (!output)
				return;

			buffer_entry entry;
			buffer_mgr__get_entry(type, buffer_id, &entry);
			*output = entry.handler;
		}

		u32 buffer_mgr__get_count(const buffer_type& type)
		{
			const auto& state = g_buffer_mgr_state;
			u32 counters[] = {
				state.vertex_buffers.count(),
				state.index_buffers.count(),
				state.constant_buffers.count()
			};
			return counters[(u8)type];
		}

		u32 buffer_mgr__get_total_count()
		{
			const auto& state = g_buffer_mgr_state;
			return state.vertex_buffers.count() +
				state.index_buffers.count() +
				state.constant_buffers.count();
		}

		void buffer_mgr__clear_cache(const buffer_type& type)
		{
			switch (type)
			{
			case buffer_type::vertex_buffer:
			{
				for (const auto& entry : g_buffer_mgr_state.vertex_buffers)
					buffer_mgr__free_buffer(entry.value);
				g_buffer_mgr_state.vertex_buffers.clear();
			}
				break;
			case buffer_type::index_buffer:
			{
				for (const auto& entry : g_buffer_mgr_state.index_buffers)
					buffer_mgr__free_buffer(entry.value);
				g_buffer_mgr_state.index_buffers.clear();
			}
				break;
			case buffer_type::constant_buffer:
			{
				for (const auto& entry : g_buffer_mgr_state.constant_buffers)
					buffer_mgr__free_buffer(entry.value);
				g_buffer_mgr_state.constant_buffers.clear();
			}
				break;
			}
		}

		void buffer_mgr__get_available_buffers(const buffer_type& type, u32* count, u16* buffers)
		{
			const auto& state = g_buffer_mgr_state;
			u32 counters[] = {
				state.vertex_buffers.count(),
				state.index_buffers.count(),
				state.constant_buffers.count()
			};

			*count = counters[(u8)type];
			if (buffers == null)
				return;

			const core::pool_entry<buffer_entry, u16>* buffer_data[] = {
				state.vertex_buffers.data(),
				state.index_buffers.data(),
				state.constant_buffers.data()
			};

			for (u32 i = 0; i < *count; ++i)
				buffers[i] = buffer_data[(u8)type][i].id;
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

		void buffer_mgr__get_entry(const buffer_type& type, u16 id, buffer_entry* output)
		{
			switch (type)
			{
				case buffer_type::vertex_buffer:
				{
					if(id != no_vertex_buffer)
						*output = g_buffer_mgr_state.vertex_buffers[id].value;
				}
					break;
				case buffer_type::index_buffer:
				{
					if(id != no_index_buffer)
						*output = g_buffer_mgr_state.index_buffers[id].value;
				}
					break;
				case buffer_type::constant_buffer:
				{
					if(id != no_constant_buffer)
						*output = g_buffer_mgr_state.constant_buffers[id].value;
				}
					break;
			}
		}
	}
}