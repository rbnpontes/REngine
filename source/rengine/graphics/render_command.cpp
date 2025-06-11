#include "./render_command.h"
#include "./render_command_private.h"

#include "../exceptions.h"
#include "../core/allocator.h"
#include "../core/string_pool.h"

#include <fmt/format.h>

namespace rengine {
	namespace graphics {
		void render_command_begin(c_str cmd_name)
		{
			auto& state = g_render_command_state;
			state.tmp_cmd_data.name = cmd_name;
			state.curr_cmd = &g_render_command_state.tmp_cmd_data;
			state.is_updating = false;
		}

		void render_command_begin_update(const render_command_t& command)
		{
			auto& state = g_render_command_state;
			const auto& it = state.commands.find_as(command);
			if (it == state.commands.end())
				return;

			state.is_updating = true;
			state.curr_cmd = it->second.get();
		}

		render_command_t render_command_end()
		{
			auto& state = g_render_command_state;
			auto curr_cmd = *state.curr_cmd;

			render_command__prepare(curr_cmd);

			const auto& it = state.commands.find_as(curr_cmd.id);
			if (it != state.commands.end())
				return state.curr_cmd->id;

			if (state.num_commands == GRAPHICS_MAX_RENDER_COMMANDS)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_render_cmd_cant_build_render_cmd, GRAPHICS_MAX_RENDER_COMMANDS).c_str()
				);

			render_command__build_internal_objects(curr_cmd);

			auto cmd = shared_ptr<render_command_data>(core::alloc_new<render_command_data>());
			*cmd = curr_cmd;
			state.commands[cmd->id] = cmd;
			++state.num_commands;
			state.curr_cmd = null;
			state.is_updating = false;
			return cmd->id;
		}

		void render_command_destroy(const render_command_t& command)
		{
			auto& state = g_render_command_state;
			const auto& it = state.commands.find_as(command);

			if (it == state.commands.end())
				return;

			state.commands.erase(it);
			--state.num_commands;
		}

		void render_command_set_vbuffer(const vertex_buffer_t& buffer, const u64& offset)
		{
			render_command_set_vbuffers(&buffer, 1, &offset);
		}

		void render_command_set_vbuffers(const vertex_buffer_t* buffers, u8 num_buffers, const u64* offsets)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_vbuffers(cmd, buffers, num_buffers, offsets);
		}

		void render_command_set_ibuffer(const index_buffer_t& buffer, const u64& offset)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_ibuffer(cmd, buffer, offset);
		}

		void render_command_set_rt(const render_target_t& rt_id, const render_target_t& depth_id)
		{
			render_command_set_rts(&rt_id, 1, depth_id);
		}

		void render_command_set_rts(const render_target_t* render_targets, u8 num_rts, const render_target_t& depth_id)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_rts(cmd, render_targets, num_rts, depth_id);
		}

		void render_command_set_tex2d(c_str slot_name, const texture_2d_t& tex_id)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			core::hash_t slot_hash{};
			core::string_pool_intern(slot_name, &slot_hash);
			render_command_set_tex2d(slot_hash, tex_id);
		}

		void render_command_set_tex3d(c_str slot_name, const texture_3d_t& tex_id)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			core::hash_t slot_hash{};
			core::string_pool_intern(slot_name, &slot_hash);
			render_command_set_tex3d(slot_hash, tex_id);
		}

		void render_command_set_texcube(c_str slot_name, const texture_cube_t& tex_id)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			core::hash_t slot_hash{};
			core::string_pool_intern(slot_name, &slot_hash);
			render_command_set_texcube(slot_hash, tex_id);
		}

		void render_command_set_texarray(c_str slot_name, const texture_array_t& tex_id)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			core::hash_t slot_hash{};
			core::string_pool_intern(slot_name, &slot_hash);
			render_command_set_texarray(slot_hash, tex_id);
		}

		void render_command_set_tex2d(core::hash_t slot, const texture_2d_t& tex_id)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_tex2d(cmd, slot, tex_id);
		}

		void render_command_set_tex3d(core::hash_t slot, const texture_3d_t& tex_id)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_tex3d(cmd, slot, tex_id);
		}

		void render_command_set_texcube(core::hash_t slot, const texture_cube_t& tex_id)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_texcube(cmd, slot, tex_id);
		}

		void render_command_set_texarray(core::hash_t slot, const texture_array_t& tex_id)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_texarray(cmd, slot, tex_id);
		}

		void render_command_unset_tex(core::hash_t slot)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__unset_tex(cmd, slot);
		}

		void render_command_set_viewport(const math::urect& rect)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_viewport(cmd, rect);
		}

		void render_command_set_topology(const primitive_topology& topology)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_topology(cmd, topology);
		}

		void render_command_set_cull_mode(const cull_mode& cull)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_cull(cmd, cull);
		}

		void render_command_set_vertex_elements(const u32& vertex_elements)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_vertex_elements(cmd, vertex_elements);
		}

		void render_command_set_program(const shader_program_t& program_id)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_program(cmd, program_id);
		}

		void render_command_set_depth_enabled(const bool& enabled)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_depth_enabled(cmd, enabled);
		}

		void render_command_set_wireframe(const bool& enabled)
		{
			ASSERT_RENDER_COMMAND_UPDATE();
			auto& cmd = *g_render_command_state.curr_cmd;
			render_command__set_wireframe(cmd, enabled);
		}
	}
}