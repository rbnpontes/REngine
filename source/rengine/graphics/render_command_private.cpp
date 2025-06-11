#include "./render_command_private.h"
#include "./pipeline_state_manager.h"
#include "./render_target_manager.h"
#include "./srb_manager.h"
#include "./texture_manager.h"
#include "./shader_manager_private.h"

#include "../strings.h"
#include "../exceptions.h"
#include "../core/hash.h"
#include "../core/string_pool.h"
#include "../core/allocator.h"

#include <fmt/format.h>
namespace rengine {
	namespace graphics {
		render_command_state g_render_command_state = {};

		void render_command__init()
		{
			g_render_command_state.log = io::logger_use(strings::logs::g_render_cmd_tag);
		}

		void render_command__assert_update()
		{
			if (g_render_command_state.curr_cmd)
				return;

			throw graphics_exception(
				strings::exceptions::g_render_cmd_call_begin_first
			);
		}

		void render_command__prepare(render_command_data& data)
		{
			render_command__prepare_textures(data);
			render_command__build_hash(data);
		}

		void render_command__build_internal_objects(render_command_data& data)
		{
			render_command__build_pipeline(data);
			render_command__build_srb(data);
		}

		void render_command__build_pipeline(render_command_data& data)
		{
			auto immutable_samplers = (immutable_sampler_desc*)core::alloc_scratch(
				sizeof(immutable_sampler_desc) * GRAPHICS_MAX_BOUND_TEXTURES
			);
			const auto name = fmt::format("{0}::gpipeline", data.name);
			graphics_pipeline_state_create pipeline_create;
			pipeline_create.name = name.c_str();
			pipeline_create.cull = data.cull;
			pipeline_create.depth = data.depth_enabled;
			pipeline_create.wireframe = data.wireframe;
			pipeline_create.topology = data.topology;
			pipeline_create.shader_program = data.program;
			// TODO: implement scissors
			// pipeline_create.scissors = cmd.scissors;
			pipeline_create.num_render_targets = data.num_render_targets;

			render_target_desc rt_desc;
			for (u8 i = 0; i < data.num_render_targets; ++i) {
				render_target_mgr_get_desc(data.render_targets[i], &rt_desc);
				pipeline_create.render_target_formats[i] = rt_desc.format;

				if (i == 0 && render_target_mgr_get_type(data.render_targets[0]) == render_target_type::multisampling)
					pipeline_create.msaa_level = rt_desc.sample_count;
			}

			if (data.depth_stencil != no_render_target) {
				render_target_mgr_get_desc(data.depth_stencil, &rt_desc);
				pipeline_create.depth_stencil_format = rt_desc.depth_format;
			}

			pipeline_create.immutable_samplers = immutable_samplers;
			pipeline_create.num_immutable_samplers = 0;
			u32 num_immutable_samplers = 0;
			for (auto& res_it : data.resources) {
				auto& res = res_it.second;
				if (res.resource.type != resource_type::cbuffer)
					continue;

				auto& immutable_sampler = pipeline_create.immutable_samplers[num_immutable_samplers];
				immutable_sampler.name = res.resource.name;
				immutable_sampler.shader_type_flags = res.resource.shader_flags;
				++pipeline_create.num_immutable_samplers;
			}

			data.pipeline_state = pipeline_state_mgr_create_graphics(pipeline_create);

			core::alloc_scratch_pop(
				sizeof(immutable_sampler_desc) * GRAPHICS_MAX_BOUND_TEXTURES
			);
		}

		void render_command__build_srb(render_command_data& data)
		{
			// TODO: recycle SRB instead of create new ones
			srb_mgr_create_desc srb_desc;
			srb_desc.pipeline = data.pipeline_state;
			srb_desc.num_resources = 0;
			srb_desc.resources = (srb_mgr_resource_desc*)core::alloc_scratch(
				sizeof(srb_mgr_resource_desc) * data.resources.size()
			);

			for (const auto& it : data.resources) {
				const auto& res = it.second;
				auto& srb_res = srb_desc.resources[srb_desc.num_resources];
				srb_res.name = res.resource.name;
				srb_res.id = res.tex_id;
				srb_res.type = res.type;
				++srb_desc.num_resources;
			}
			data.srb = srb_mgr_create(srb_desc);

			core::alloc_scratch_pop(sizeof(srb_mgr_resource_desc) * data.resources.size());
		}

		void render_command__build_hash(render_command_data& data)
		{
			auto& hashes = data.hashes;
			render_command__build_vbuffer_hash(data);
			render_command__build_ibuffer_hash(data);
			render_command__build_rts_hash(data);
			render_command__build_viewport_hash(data);
			render_command__build_texture_hash(data);
			// calculate graphics state hashes
			hashes.graphics_state = data.vertex_elements;
			hashes.graphics_state = core::hash_combine(hashes.graphics_state, (u32)data.topology);
			hashes.graphics_state = core::hash_combine(hashes.graphics_state, (u32)data.cull);
			hashes.graphics_state = core::hash_combine(hashes.graphics_state, (u32)data.wireframe);
			hashes.graphics_state = core::hash_combine(hashes.graphics_state, (u32)data.depth_enabled);

			data.id = core::hash_combine(hashes.render_targets, hashes.vertex_buffers);
			data.id = core::hash_combine(data.id, hashes.vertex_buffer_offsets);
			data.id = core::hash_combine(data.id, hashes.index_buffer);
			data.id = core::hash_combine(data.id, hashes.viewport);
			data.id = core::hash_combine(data.id, hashes.graphics_state);
		}

		void render_command__build_vbuffer_hash(render_command_data& cmd)
		{
			core::hash_t vbuffer_hash = cmd.num_vertex_buffers;
			core::hash_t offsets_hash = cmd.num_vertex_buffers;
			for (u8 i = 0; i < cmd.num_vertex_buffers; ++i) {
				vbuffer_hash = core::hash_combine(vbuffer_hash, cmd.vertex_buffers[i]);
				// TODO: remove offsets from hash check
				offsets_hash = core::hash_combine(offsets_hash, cmd.vertex_offsets[i]);
			}

			cmd.hashes.vertex_buffers = vbuffer_hash;
			cmd.hashes.vertex_buffer_offsets = offsets_hash;
		}

		void render_command__build_ibuffer_hash(render_command_data& cmd)
		{
			// TODO: remove offsets from hash check
			cmd.hashes.index_buffer = core::hash_combine(cmd.index_buffer, cmd.index_offset);
		}

		void render_command__build_rts_hash(render_command_data& cmd)
		{
			core::hash_t hash = cmd.num_render_targets;
			for (u8 i = 0; i < cmd.num_render_targets; ++i)
				hash = core::hash_combine(hash, cmd.render_targets[i]);
			cmd.hashes.render_targets = core::hash_combine(hash, cmd.depth_stencil);
		}

		void render_command__build_viewport_hash(render_command_data& cmd)
		{
			cmd.hashes.viewport = cmd.viewport.to_hash();
		}

		void render_command__build_texture_hash(render_command_data& cmd)
		{
			// only build texture hash if shader program is set
			if (cmd.program == no_shader_program)
				return;

			auto hash = (core::hash_t)cmd.resources.size();
			for (const auto& it : cmd.resources) {
				const auto& res = it.second;
				hash = core::hash_combine(hash, res.resource.id);
				hash = core::hash_combine(hash, (u32)res.type);
			}

			cmd.hashes.textures = hash;
		}

		void render_command__prepare_textures(render_command_data& cmd)
		{
			if (cmd.program == no_shader_program)
				return;

			// check bounded textures with shader resources
			// if some textures are not bounded, then we need to bind a
			// dummy texture instead.
			auto& state = g_render_command_state;

			shader_program program;
			shader_mgr__get_program(cmd.program, &program);

			hash_map<core::hash_t, render_command_resource> new_textures;
			for (auto i = 0; i < program.num_resources; ++i) {
				const auto& res = program.resources[i];
				if (res.type == resource_type::cbuffer)
					continue;

				if (res.type != resource_type::tex2d)
					throw not_implemented_exception();

				const auto& bounded_tex_it = cmd.resources.find_as(res.id);
				if (cmd.resources.end() == bounded_tex_it) {
					new_textures[res.id] = render_command_resource{
						.type = res.type,
						.resource = res,
						.tex_id = texture_mgr_get_white_dummy_tex2d(),
					};
					continue;
				}

				new_textures[res.id] = bounded_tex_it->second;
			}

			cmd.resources = new_textures;
		}

		void render_command__set_vbuffers(render_command_data& cmd, const vertex_buffer_t* buffers, u8 num_buffers, const u64* offsets)
		{
			auto& state = g_render_command_state;
			auto log = state.log;

			if (num_buffers > GRAPHICS_MAX_VBUFFERS) {
				num_buffers = GRAPHICS_MAX_VBUFFERS;

				log->warn(fmt::format(strings::logs::g_render_cmd_isnt_allowed_to_set_buffer_grt_than_max,
					num_buffers,
					GRAPHICS_MAX_VBUFFERS).c_str()
				);
			}

			memcpy(cmd.vertex_buffers.data(), buffers, sizeof(vertex_buffer_t) * num_buffers);
			memcpy(cmd.vertex_offsets.data(), offsets, sizeof(u64) * num_buffers);
			cmd.num_vertex_buffers = num_buffers;
		}

		void render_command__set_ibuffer(render_command_data& cmd, const index_buffer_t& buffer, const u64& offset)
		{
			cmd.index_buffer = buffer;
			cmd.index_offset = offset;
		}

		void render_command__set_rts(render_command_data& cmd, const render_target_t* rts, u8 num_rts, const render_target_t& depth_id)
		{
			auto& state = g_render_command_state;
			auto log = state.log;

			if (num_rts > GRAPHICS_MAX_RENDER_TARGETS) {
				num_rts = GRAPHICS_MAX_RENDER_TARGETS;

				log->warn(
					fmt::format(strings::logs::g_render_isnt_allowed_to_set_rt_grt_than_max,
						num_rts,
						GRAPHICS_MAX_RENDER_TARGETS).c_str()
				);
			}

			for (u8 i = 0; i < num_rts; ++i) {
				if (cmd.render_targets[i] != rts[i])
					cmd.pipeline_state = no_pipeline_state;
				cmd.render_targets[i] = rts[i];
			}

			if (cmd.num_render_targets != num_rts)
				cmd.pipeline_state = no_pipeline_state;

			cmd.num_render_targets = num_rts;
			cmd.depth_stencil = depth_id;
		}

		void render_command__set_tex2d(render_command_data& cmd, const core::hash_t& slot, const texture_2d_t& id)
		{
			if (no_texture_2d == id)
				return;

			render_command_resource res{};
			res.type = resource_type::tex2d;
			res.tex_id = id;

			cmd.resources[slot] = res;
			cmd.srb = no_srb;
		}

		void render_command__set_tex3d(render_command_data& cmd, const core::hash_t& slot, const texture_3d_t& id)
		{
			if (no_texture_3d == id)
				return;

			render_command_resource res{};
			res.type = resource_type::tex3d;
			res.tex_id = id;

			cmd.resources[slot] = res;
			cmd.srb = no_srb;
		}

		void render_command__set_texcube(render_command_data& cmd, const core::hash_t& slot, const texture_cube_t& id)
		{
			if (no_texture_cube == id)
				return;

			render_command_resource res{};
			res.type = resource_type::texcube;
			res.tex_id = id;

			cmd.resources[slot] = res;
			cmd.srb = no_srb;
		}

		void render_command__set_texarray(render_command_data& cmd, const core::hash_t& slot, const texture_array_t& id)
		{
			if (no_texture_array == id)
				return;

			render_command_resource res{};
			res.type = resource_type::texarray;
			res.tex_id = id;

			cmd.resources[slot] = res;
			cmd.srb = no_srb;
		}

		void render_command__unset_tex(render_command_data& cmd, const core::hash_t& slot)
		{
			const auto& it = cmd.resources.find_as(slot);
			if (cmd.resources.end() == it)
				return;
			cmd.resources.erase(it);
		}

		void render_command__set_viewport(render_command_data& cmd, const math::urect& rect)
		{
			auto& state = g_render_command_state;

			cmd.viewport = rect;
		}

		void render_command__set_topology(render_command_data& cmd, const primitive_topology& topology)
		{
			auto& state = g_render_command_state;

			if (cmd.topology != topology)
				cmd.pipeline_state = no_pipeline_state;
			cmd.topology = topology;
		}

		void render_command__set_cull(render_command_data& cmd, const cull_mode& cull)
		{
			if (cmd.cull != cull)
				cmd.pipeline_state = no_pipeline_state;
			cmd.cull = cull;
		}

		void render_command__set_program(render_command_data& cmd, const shader_t& program_id)
		{
			cmd.program = program_id;
		}

		void render_command__set_depth_enabled(render_command_data& cmd, const bool& enabled)
		{
			if (cmd.depth_enabled != enabled)
				cmd.pipeline_state = no_pipeline_state;
			cmd.depth_enabled = enabled;
		}

		void render_command__set_wireframe(render_command_data& cmd, const bool& enabled)
		{
			if (cmd.wireframe != enabled)
				cmd.pipeline_state = no_pipeline_state;
			cmd.wireframe = enabled;
		}

		bool render_command__get(const render_command_t& cmd_id, render_command_data* data)
		{
			if (!data)
				return false;

			auto& state = g_render_command_state;
			auto log = state.log;

			auto it = state.commands.find(cmd_id);
			if (it != state.commands.end()) {
				*data = *it->second;
				return true;
			}

			log->warn(fmt::format(strings::logs::g_render_cmd_not_found_command, cmd_id).c_str());
			return false;
		}
	}
}