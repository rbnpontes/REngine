#include "./pipeline_state_manager.h"
#include "./pipeline_state_manager_private.h"

#include "../exceptions.h"
#include "../strings.h"
#include "../core/hash.h"

namespace rengine {
    namespace graphics {
        pipeline_state_t pipeline_state_mgr_create_graphics(const graphics_pipeline_state_create& create_info)
        {
            const auto pipeline_id = pipeline_state_mgr_graphics_hash_desc(create_info);
            const auto& it = g_cached_pipelines.find_as(pipeline_id);
            if (it != g_cached_pipelines.end())
                return pipeline_id;

            const auto pipeline = pipeline_state_mgr__create_graphics(create_info);
            if (!pipeline)
                throw graphics_exception(strings::exceptions::g_shader_mgr_fail_to_create_shader);

            g_cached_pipelines[pipeline_id] = pipeline;
            ++g_cached_pipelines_count;
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
            result = core::hash_combine(result, create_info.vertex_elements);
            result = core::hash_combine(result, create_info.depth);
            return core::hash_combine(result, create_info.wireframe);
        }

        ptr pipeline_state_mgr_get_internal_handle(pipeline_state_t id)
        {
            const auto& it = g_cached_pipelines.find_as(id);
            if (it == g_cached_pipelines.end())
                return null;
            return it->second;
        }

        u32 pipeline_state_mgr_get_cache_count()
        {
            return g_cached_pipelines_count;
        }

        void pipeline_state_mgr_clear_cache()
        {
            for (const auto& it : g_cached_pipelines)
                it.second->Release();

            g_cached_pipelines_count = 0;
        }
    }
}
