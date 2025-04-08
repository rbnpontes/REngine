#pragma once
#include <rengine/types.h>

namespace rengine {
    namespace strings {
        constexpr static c_str g_empty = "";
        constexpr static c_str g_backend_strings[] = {
            "d3d11",
            "d3d12",
            "vulkan",
            "webgpu",
            "opengl",
            "unknow"
        };
        constexpr static c_str g_buffer_names[] = {
            "vertex buffer",
            "index buffer",
            "constant buffer"
        };

        namespace graphics {
            constexpr static c_str g_models_vbuffer_name = "rengine::models::vbuffer";
            constexpr static c_str g_models_ibuffer_name = "rengine::models::ibuffer";
			constexpr static c_str g_models_pipeline_name = "rengine::models::gpipeline";
			constexpr static c_str g_models_vshader_name = "rengine::models::vshader";
			constexpr static c_str g_models_pshader_name = "rengine::models::pshader";

            constexpr static c_str g_viewport_rt_name = "rengine::viewport";
            constexpr static c_str g_default_cmd_name = "rengine::render_command";

            namespace shaders {
                constexpr static c_str g_model_nouv_vertex = R"(
                    struct vs_input {
                        float3 position : ATTRIB0;
                        uint color      : ATTRIB3;
                    };
   
                    struct vs_output {
                        float4 position : SV_Position;
                        float4 color    : COLOR0;
                    };

                    vs_output main(in vs_input input) {
                        vs_output output = (vs_output)0;
                        output.position = float4(input.position, 1.0f);

                        float4 color = float4(
                            (float)((input.color >> 0) & 0xFF),
                            (float)((input.color >> 8) & 0xFF),
                            (float)((input.color >> 16) & 0xFF),
                            (float)((input.color >> 24) & 0xFF)
                        );
                        color /= float4(255.0f, 255.0f, 255.0f, 255.0f);
                        output.color = color;
                        return output;
                    }   
                )";
                constexpr static c_str g_model_uv_vertex = R"(
                    struct vs_input {
                        float3 position : ATTRIB0;
                        float4 color    : ATTRIB3;
                        float2 uv       : ATTRIB4;
                    };
   
                    struct vs_output {
                        float4 position : SV_Position;
                        float2 uv       : TEXCOORD0;
                        float4 color    : COLOR0;
                    };
                    vs_output main(in vs_input input) {
                        vs_output output = (vs_output)0;
                        output.position = float4(input.position, 1.0f);
                        output.uv = input.uv;
                        output.color = input.color;
                        return output;
                    }
                )";

				constexpr static c_str g_model_nouv_pixel = R"(
                    struct ps_input {
                        float4 position : SV_Position;
                        float4 color    : COLOR0;
                    };
                    float4 main(in ps_input input) : SV_Target {
                        return input.color;
                    }
                )";
            }
        }

        namespace logs {
            constexpr static c_str g_engine_tag = "rengine";
            constexpr static c_str g_graphics_tag = "graphics";
            constexpr static c_str g_diligent_tag = "diligent";
            constexpr static c_str g_buffer_mgr_tag = "buffer_mgr";
            constexpr static c_str g_renderer_tag = "renderer";

            constexpr static c_str g_logger_fmt = "[{0}/{1}/{2} {3}:{4}:{5}][{6}][{7}]: {8}";

            constexpr static c_str g_engine_already_stopped = "Engine is already stopped";

            constexpr static c_str g_graphics_invalid_adapter_id = "Invalid adapter id {0}. Engine will try to select a best match device";
            constexpr static c_str g_graphics_no_suitable_device_found = "No suitable device found, using first available.";
            constexpr static c_str g_graphics_swapchain_has_been_created = "SwapChain has been created for window {0}";
            constexpr static c_str g_graphics_diligent_dbg_fmt = "{0} | Function: {1} | File: {2} | Line: {3}";

            constexpr static c_str g_buffer_mgr_cbuffer_must_be_dyn = "Constant buffer must be dynamic. Forcing to dynamic!";
            constexpr static c_str g_buffer_mgr_cant_update_non_dyn = "Failed to update buffer. Is not possible to update a non-dynamic buffer, "
                "engine will skip this operation! Buffer Id = {0}, Buffer Name = {1}, Buffer Type = {2}";
            constexpr static c_str g_buffer_mgr_update_data_size_is_greater_than_buffer = "Upload data size ({0}) is greater than Buffer '{2}' size ({3}). "
                "Engine will copy partial data to GPU, next time try to increate buffer size! "
                "Upload Size = {0}, Buffer Id = {1} Buffer Name = {2}, Buffer Size = {3}, Buffer Type = {4}";
            constexpr static c_str g_buffer_mgr_free_invalid_buffer = "Can´t free an invalid buffer. Buffer Id = {0}";
        
            constexpr static c_str g_rt_mgr_cant_destroy_invalid_id = "Can't destroy render target from invalid id. Id = {0}";

            constexpr static c_str g_renderer_cant_clear_unset_depthbuffer = "Can't clear depth buffer that has not been set.";
            constexpr static c_str g_renderer_isnt_allowed_to_set_rt_grt_than_max = "Number of render targets ({0}) is greater than max allowed ({1})";
            constexpr static c_str g_renderer_isnt_allowed_to_set_buffer_grt_than_max = "Number of vertex buffer ({0}) is greater than max allowed ({1})";
            constexpr static c_str g_renderer_not_found_command = "Not found command from given id {0}";
        }

        namespace exceptions {
            constexpr static c_str g_null_object = "{0} is null";

            constexpr static c_str g_window_invalid_id = "Invalid window id";
            constexpr static c_str g_window_reached_max_created_windows = "Reached max of created windows";

			constexpr static c_str g_logger_reached_max_log_objects = "Reached max of created log objects.";

            constexpr static c_str g_graphics_unsupported_backend = "Unsupported graphics backend {0} on this platform";
            constexpr static c_str g_graphics_not_initialized = "Graphics is not initialized";
            constexpr static c_str g_graphics_not_suitable_device = "Not found a suitable graphics card device";
            constexpr static c_str g_graphics_unknow_adapter = "Unknow adapter. It seems that you choose a unknow adapter.";
            constexpr static c_str g_graphics_opengl_doesnt_support_swapchain = "OpenGL doesn't support SwapChain";
            constexpr static c_str g_graphics_fail_to_create_g_objects = "Failed to create graphics objects";
            constexpr static c_str g_graphics_fail_to_create_swapchain = "Failed to create SwapChain";
            
            constexpr static c_str g_shader_mgr_fail_to_create_shader = "Failed to create shader object";
            constexpr static c_str g_pipeline_state_mgr_fail_to_create_gpipeline = "Failed to create graphics pipeline object";

            constexpr static c_str g_buffer_mgr_requires_initial_data = "Non dynamic buffers requires an initial data";
            constexpr static c_str g_buffer_mgr_fail_to_create_buffer = "Failed to create {0}";
            constexpr static c_str g_buffer_mgr_reach_limit = "Failed to create {0}. Reached limit of {1} buffers";
            constexpr static c_str g_buffer_mgr_invalid_id = "Invalid buffer id";
            constexpr static c_str g_buffer_mgr_failed_to_update_buffer = "Failed to update buffer. Buffer Id = {0}, Buffer Name = {1}, Buffer Type = {2}";
            constexpr static c_str g_buffer_mgr_cant_realloc_non_dyn = "Failed to realloc buffer. Is not possible to realloc a non-dynamic buffer, "
                "You must free this buffer ({0}) and create again with different size! "
                "Buffer Id = {0}, Buffer Name = {1}, Buffer Type = {2}";

			constexpr static c_str g_rt_mgr_reach_limit = "Failed to create render target. Reached limit of {0} render targets";
            constexpr static c_str g_rt_mgr_failed_to_create = "Failed to create render target";
			constexpr static c_str g_rt_mgr_failed_to_create_depthbuffer = "Failed to create depth buffer";
            constexpr static c_str g_rt_mgr_invalid_id = "Invalid Render Target Id ({0})";
            constexpr static c_str g_rt_mgr_cant_external = "Is not possible to resize external render target. Id = {0}";

            constexpr static c_str g_renderer_rt_idx_grt_than_max = "Render Target Index is greater than the max supported render targets {0}";
            constexpr static c_str g_renderer_rt_idx_grt_than_set = "Render Target Index ({0}) is greater than set render targets ({1})";
            constexpr static c_str g_renderer_clear_depth_without_set = "Can´t clear Depth Stencil. You must assign depth stencil first";
            constexpr static c_str g_renderer_cant_build_render_cmd = "Failed to create render command. Reached limit of {0} render commands";
        
            constexpr static c_str g_models_failed_to_alloc_vbuffer = "Failed to allocate vertex buffer with size {0}";
            constexpr static c_str g_models_failed_to_alloc_ibuffer = "Failed to allocate index buffer with size {0}";
        }
    }
}