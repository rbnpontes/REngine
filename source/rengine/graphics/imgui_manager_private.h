#include "../base_private.h"
#include "../events/window_events.h"
#include "../core/window.h"

#include <imgui/imgui.h>
#include <SDL3/SDL.h>

namespace rengine {
	namespace graphics {
		struct imgui_manager_state {
			ImGuiContext* ctx{ null };
			string backend_name{};

			SDL_Cursor* cursors[(u8)ImGuiMouseCursor_COUNT]{};
			SDL_Cursor* last_cursor{ null };

			texture_2d_t font_tex{ UINT16_MAX };
			shader_t vertex_shader{ no_shader };
			shader_t pixel_shader{ no_shader };

			vertex_buffer_t vertex_buffer{ no_vertex_buffer };
			index_buffer_t index_buffer{ no_index_buffer };

			u32 curr_vbuffer_count{ 0 };
			u32 curr_ibuffer_count{ 0 };
		};
		extern imgui_manager_state g_imgui_manager_state;

		ptr imgui_manager__malloc(size_t size, ptr user_data);
		void imgui_manager__free(ptr _ptr, ptr user_data);

		void imgui_manager__init();
		void imgui_manager__deinit();
		
		void imgui_manager__init_font_tex();
		void imgui_manager__init_shaders();

		c_str imgui_manager__get_clipboard_text(ImGuiContext* ctx);
		void imgui_manager__set_clipboard_text(ImGuiContext* ctx, c_str text);
		bool imgui_manager__open_in_shell(ImGuiContext* ctx, c_str url);

		void imgui_manager__setup_platform_handle();
		void imgui_manager__on_sdl_event(events::window_event_args& args);
		void imgui_manager__update();
		void imgui_manager__update_mouse_data(const core::window_t wnd_id, const core::window_desc_t& wnd_desc);
		void imgui_manager__update_mouse_cursor();

		void imgui_manager__render();
		void imgui_manager__copy_buffers(ImDrawData* draw_data);
		void imgui_manager__setup_render_state();
		void imgui_manager__render_commands(ImDrawData* draw_data);
	}
}