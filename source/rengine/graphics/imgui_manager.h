#pragma once
#include <rengine/api.h>
#include <imgui/imgui.h>

namespace rengine {
	namespace graphics {
		namespace internal {
			R_EXPORT ImGuiContext* imgui_manager__get_context();
			R_EXPORT bool imgui_manager__can_begin();
			R_EXPORT void imgui_manager__get_allocator_functions(ImGuiMemAllocFunc* alloc_func, ImGuiMemFreeFunc* free_func);
			R_EXPORT void imgui_manager__do_begin();
		}

		inline bool imgui_manager_begin() {
			if (!internal::imgui_manager__can_begin())
				return false;

			ImGuiMemAllocFunc alloc_func;
			ImGuiMemFreeFunc free_func;

			internal::imgui_manager__get_allocator_functions(&alloc_func, &free_func);
			internal::imgui_manager__do_begin();

			ImGui::SetCurrentContext(internal::imgui_manager__get_context());
			ImGui::SetAllocatorFunctions(
				alloc_func,
				free_func
			);
			ImGui::NewFrame();
			return true;
		}

		R_EXPORT void imgui_manager_end();
	}
}