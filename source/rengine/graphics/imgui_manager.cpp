#include "imgui_manager.h"
#include "./imgui_manager_private.h"
#include "../rengine_private.h"
#include "../core/window.h"

namespace rengine {
	namespace graphics {
		namespace internal {
			ImGuiContext* imgui_manager__get_context()
			{
				return g_imgui_manager_state.ctx;
			}
			
			bool imgui_manager__can_begin()
			{
				const auto& engine_state = g_engine_state;
				const auto& state = g_imgui_manager_state;
				return engine_state.window_id != core::no_window && null != state.ctx;
			}

			void imgui_manager__get_allocator_functions(ImGuiMemAllocFunc* alloc_func, ImGuiMemFreeFunc* free_func)
			{
				*alloc_func = imgui_manager__malloc;
				*free_func = imgui_manager__free;
			}

			void imgui_manager__do_begin()
			{
				imgui_manager__setup_platform_handle();
				imgui_manager__update();

				const auto& engine_state = g_engine_state;
				auto& imIO = ImGui::GetIO();
				const auto& wnd_size = core::window_get_size(engine_state.window_id);
				imIO.DisplaySize = { (float)wnd_size.x, (float)wnd_size.y };
			}
		}

		void imgui_manager_end()
		{
			const auto& engine_state = g_engine_state;
			const auto& state = g_imgui_manager_state;
			if (engine_state.window_id == core::no_window && null == state.ctx)
				return;
			ImGui::Render();
		}
	}
}
