#include "./pipeline_state_manager.h"
#include "./pipeline_state_manager_private.h"

#include "../exceptions.h"
#include "../strings.h"
#include "../core/hash.h"

namespace rengine {
    namespace graphics {
        pipeline_state_t pipeline_state_mgr_create_graphics(const graphics_pipeline_state_create& create_info)
        {
            auto& state = g_pipeline_state_mgr_state;
            const auto pipeline_id = pipeline_state_mgr_graphics_hash_desc(create_info);
            const auto& it = state.pipelines.find_as(pipeline_id);
            if (it != state.pipelines.end())
                return pipeline_id;

            const auto pipeline = pipeline_state_mgr__create_graphics(create_info);
            if (!pipeline)
                throw graphics_exception(strings::exceptions::g_shader_mgr_fail_to_create_shader);

            state.pipelines[pipeline_id] = pipeline;
            ++state.pipeline_count;
            return pipeline_id;
        }
        
        core::hash_t pipeline_state_mgr_graphics_hash_desc(const graphics_pipeline_state_create& create_info)
        {
            const u16* rt_formats = create_info.render_target_formats;
            core::hash_t result = core::hash(create_info.name);
            result = core::hash_combine(result, core::hash(rt_formats, (u32)create_info.num_render_targets));
            result = core::hash_combine(result, create_info.depth_stencil_format);
            result = core::hash_combine(result, create_info.num_render_targets);
            result = core::hash_combine(result, (u32)create_info.topology);
            result = core::hash_combine(result, (u32)create_info.cull);
            result = core::hash_combine(result, (u32)create_info.blend_mode);
            result = core::hash_combine(result, create_info.constant_depth_bias);
            result = core::hash_combine(result, create_info.slope_scaled_depth_bias);
            const auto& dd = create_info.depth_desc;
            result = core::hash_combine(result, (u32)dd.depth_enabled);
            result = core::hash_combine(result, (u32)dd.depth_write);
            result = core::hash_combine(result, (u32)dd.stencil_test);
            result = core::hash_combine(result, (u32)dd.depth_cmp_func);
            result = core::hash_combine(result, (u32)dd.stencil_cmp_func);
            result = core::hash_combine(result, (u32)dd.on_passed);
            result = core::hash_combine(result, (u32)dd.on_stencil);
            result = core::hash_combine(result, (u32)dd.on_depth_fail);
            result = core::hash_combine(result, dd.cmp_mask);
            result = core::hash_combine(result, dd.write_mask);
            result = core::hash_combine(result, create_info.color_write);
            result = core::hash_combine(result, create_info.alpha_to_coverage);
            result = core::hash_combine(result, create_info.wireframe);
            result = core::hash_combine(result, create_info.num_immutable_samplers);
			for (u32 i = 0; i < create_info.num_immutable_samplers; ++i) {
				const auto& sampler = create_info.immutable_samplers[i];
				result = core::hash_combine(result, core::hash(sampler.name));
				result = core::hash_combine(result, (u32)sampler.shader_type_flags);
                result = core::hash_combine(result, (u32)sampler.desc.filter);
                result = core::hash_combine(result, (u32)sampler.desc.address);
                result = core::hash_combine(result, (u32)sampler.desc.lod_bias);
                result = core::hash_combine(result, (u32)sampler.desc.min_lod);
                result = core::hash_combine(result, (u32)sampler.desc.max_lod);
                result = core::hash_combine(result, (u32)sampler.desc.max_anisotropy);
                result = core::hash_combine(result, (u32)sampler.desc.comparison);
			}
            return result;
        }

        ptr pipeline_state_mgr_get_internal_handle(pipeline_state_t id)
        {
            Diligent::IPipelineState* pipeline;
            pipeline_state_mgr__get_internal_handle(id, &pipeline);
            return pipeline;
        }

        u32 pipeline_state_mgr_get_cache_count()
        {
            return g_pipeline_state_mgr_state.pipeline_count;
        }

        void pipeline_state_mgr_clear_cache()
        {
            for (const auto& it : g_pipeline_state_mgr_state.pipelines)
                it.second->Release();

            g_pipeline_state_mgr_state.pipeline_count = 0;
        }
    }
}
