#pragma once
#include "../base_private.h"
#include "./render_command.h"
#include "./texture_manager.h"
#include "./shader_manager.h"
#include "./srb_manager.h"
#include "./pipeline_state_manager.h"

#include "../math/math-types.h"
#include "../io/logger.h"

namespace rengine {
	namespace graphics {
		struct render_command_hashes {
			core::hash_t render_targets{ 0 };
			core::hash_t vertex_buffers{ 0 };
			core::hash_t vertex_buffer_offsets{ 0 };
			core::hash_t index_buffer{ 0 };
                       core::hash_t viewport{ 0 };
                       core::hash_t scissors{ 0 };
                       core::hash_t graphics_state{ 0 };
			core::hash_t textures{ 0 };
			core::hash_t depth_desc{ 0 };
		};

		struct render_command_resource {
			resource_type type{ resource_type::unknow };
			shader_resource resource{};
			entity tex_id{ 0 };
		};

		struct render_command_data {
			u32 id{ 0 };
			string name{ strings::graphics::g_default_cmd_name };

			array<render_target_t, GRAPHICS_MAX_RENDER_TARGETS> render_targets{};
			array<vertex_buffer_t, GRAPHICS_MAX_VBUFFERS> vertex_buffers{};
			array<u64, GRAPHICS_MAX_VBUFFERS> vertex_offsets{};
			index_buffer_t index_buffer{};
			u64 index_offset;
			render_target_t depth_stencil{ no_render_target };
			u8 num_render_targets{ 0 };
			u8 num_vertex_buffers{ 0 };

			shader_program_t program{ no_shader_program };
            hash_map<core::hash_t, render_command_resource> resources{};
            math::urect viewport{};
            array<math::rect, GRAPHICS_MAX_SCISSORS> scissor_rects{};
            u8 num_scissors{ 0 };

			primitive_topology topology{ primitive_topology::triangle_list };
			cull_mode cull{ cull_mode::clock_wise };
			blend_mode blend_mode{ blend_mode::replace };
			bool color_write{ true };
			bool alpha_to_coverage{ false };
			bool wireframe{ false };
			float constant_depth_bias{ 0.0f };
			float slope_scaled_depth_bias{ 0.0f };
			depth_desc depth_desc{};

			pipeline_state_t pipeline_state{ no_pipeline_state };
			srb_t srb{ no_srb };
			render_command_hashes hashes{};
		};
		typedef hash_map<render_command_t, shared_ptr<render_command_data>> command_list;

		struct render_command_state {
			io::ILog* log{ null };
			command_list commands;
			u32 num_commands{ 0 };
			render_command_data* curr_cmd{ null };
			bool is_updating{ false };
			render_command_data tmp_cmd_data{};
		};
		extern render_command_state g_render_command_state;

		void render_command__init();
#if ENGINE_DEBUG
		void render_command__assert_update();
#endif
		void render_command__prepare(render_command_data& data);
		void render_command__build_internal_objects(render_command_data& data);

		void render_command__build_pipeline(render_command_data& data);
		void render_command__build_srb(render_command_data& data);
		void render_command__build_hash(render_command_data& data);
		void render_command__build_vbuffer_hash(render_command_data& cmd);
		void render_command__build_ibuffer_hash(render_command_data& cmd);
        void render_command__build_rts_hash(render_command_data& cmd);
        void render_command__build_viewport_hash(render_command_data& cmd);
        void render_command__build_scissor_hash(render_command_data& cmd);
        void render_command__build_texture_hash(render_command_data& cmd);

		void render_command__prepare_textures(render_command_data& cmd);

		void render_command__set_vbuffers(render_command_data& cmd, const vertex_buffer_t* buffers, u8 num_buffers, const u64* offsets);
		void render_command__set_ibuffer(render_command_data& cmd, const index_buffer_t& buffer, const u64& offset);
		void render_command__set_rts(render_command_data& cmd, const render_target_t* rts, u8 num_rts, const render_target_t& depth_id);
		void render_command__set_tex2d(render_command_data& cmd, const core::hash_t& slot, const texture2d_t& id);
		void render_command__set_tex3d(render_command_data& cmd, const core::hash_t& slot, const texture_3d_t& id);
		void render_command__set_texcube(render_command_data& cmd, const core::hash_t& slot, const texture_cube_t& id);
		void render_command__set_texarray(render_command_data& cmd, const core::hash_t& slot, const texture_array_t& id);
		void render_command__unset_tex(render_command_data& cmd, const core::hash_t& slot);
        void render_command__set_viewport(render_command_data& cmd, const math::urect& rect);
        void render_command__set_scissor_rects(render_command_data& cmd, const math::rect* rects, u8 num_rects);
        void render_command__set_scissor_rect(render_command_data& cmd, const math::rect& rect);
		void render_command__disable_scissors(render_command_data& cmd);
        void render_command__set_topology(render_command_data& cmd, const primitive_topology& topology);
		void render_command__set_cull(render_command_data& cmd, const cull_mode& cull);
		void render_command__set_program(render_command_data& cmd, const shader_t& program_id);
		void render_command__set_depth(render_command_data& cmd, const depth_desc& desc);
		void render_command__set_blend_mode(render_command_data& cmd, const blend_mode& mode);
		void render_command__set_color_write(render_command_data& cmd, bool enabled);
		void render_command__set_alpha_to_coverage(render_command_data& cmd, bool enabled);
        void render_command__set_constant_depth_bias(render_command_data& cmd, float bias);
        void render_command__set_slope_scaled_depth_bias(render_command_data& cmd, float bias);
        void render_command__set_wireframe(render_command_data& cmd, const bool& enabled);
        
        bool render_command__get(const render_command_t& cmd_id, render_command_data* data);
	}
}

#if ENGINE_DEBUG
    #define ASSERT_RENDER_COMMAND_UPDATE() render_command__assert_update()
#else
    #define ASSERT_RENDER_COMMAND_UPDATE()
#endif