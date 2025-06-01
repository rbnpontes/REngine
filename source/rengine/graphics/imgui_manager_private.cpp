#include "imgui_manager_private.h"
#include "./graphics_private.h"
#include "../rengine_private.h"

#include "../core/window_private.h"

#include <fmt/format.h>
#include <SDL3/SDL.h>

namespace rengine {
	namespace graphics {
		imgui_manager_state g_imgui_manager_state = {};

		ptr imgui_manager__malloc(size_t size, ptr user_data)
		{
			return core::alloc(size);
		}

		void imgui_manager__free(ptr _ptr, ptr user_data)
		{
			core::alloc_free(_ptr);
		}

		void imgui_manager__init()
		{
			auto& state = g_imgui_manager_state;
			state.ctx = ImGui::CreateContext();
			state.backend_name = fmt::format(
				strings::g_imgui_backend_name,
				strings::g_backend_strings[(u8)g_graphics_state.backend]
			).c_str();

			auto& io = ImGui::GetIO();
			ImGui::SetAllocatorFunctions(
				imgui_manager__malloc,
				imgui_manager__free,
				null
			);

			io.BackendPlatformName = state.backend_name.c_str();
			io.BackendRendererName = strings::g_renderer_name;
			io.ConfigFlags |= ImGuiConfigFlags_NavEnableGamepad | ImGuiConfigFlags_NavEnableKeyboard;
			io.BackendFlags |= ImGuiBackendFlags_HasMouseCursors | ImGuiBackendFlags_HasSetMousePos;

			auto& platform_io = ImGui::GetPlatformIO();
			platform_io.Platform_GetClipboardTextFn = imgui_manager__get_clipboard_text;
			platform_io.Platform_SetClipboardTextFn = imgui_manager__set_clipboard_text;
			platform_io.Platform_OpenInShellFn = imgui_manager__open_in_shell;
			
			auto& cursors = state.cursors;
			// TODO: move SDL cursors to proper place
			cursors[ImGuiMouseCursor_Arrow]			= SDL_CreateSystemCursor(SDL_SYSTEM_CURSOR_DEFAULT);
			cursors[ImGuiMouseCursor_TextInput]		= SDL_CreateSystemCursor(SDL_SYSTEM_CURSOR_TEXT);
			cursors[ImGuiMouseCursor_ResizeAll]		= SDL_CreateSystemCursor(SDL_SYSTEM_CURSOR_MOVE);
			cursors[ImGuiMouseCursor_ResizeNS]		= SDL_CreateSystemCursor(SDL_SYSTEM_CURSOR_NS_RESIZE);
			cursors[ImGuiMouseCursor_ResizeEW]		= SDL_CreateSystemCursor(SDL_SYSTEM_CURSOR_EW_RESIZE);
			cursors[ImGuiMouseCursor_ResizeNESW]	= SDL_CreateSystemCursor(SDL_SYSTEM_CURSOR_NESW_RESIZE);
			cursors[ImGuiMouseCursor_ResizeNWSE]	= SDL_CreateSystemCursor(SDL_SYSTEM_CURSOR_NWSE_RESIZE);
			cursors[ImGuiMouseCursor_Hand]			= SDL_CreateSystemCursor(SDL_SYSTEM_CURSOR_POINTER);
			cursors[ImGuiMouseCursor_Wait]			= SDL_CreateSystemCursor(SDL_SYSTEM_CURSOR_WAIT);
			cursors[ImGuiMouseCursor_Progress]		= SDL_CreateSystemCursor(SDL_SYSTEM_CURSOR_PROGRESS);
			cursors[ImGuiMouseCursor_NotAllowed]	= SDL_CreateSystemCursor(SDL_SYSTEM_CURSOR_NOT_ALLOWED);
			
			ImGui::StyleColorsDark();

			io.Fonts->AddFontDefault();
			imgui_manager__init_font_tex();
			events::window_subscribe_event(imgui_manager__on_sdl_event);
		}

		void imgui_manager__deinit()
		{
			events::window_unsubscribe_event(imgui_manager__on_sdl_event);

			auto& state = g_imgui_manager_state;
			ImGui::DestroyContext(state.ctx);
			state.ctx = null;
		}

		void imgui_manager__init_font_tex()
		{
			auto& state = g_imgui_manager_state;
			auto& io = ImGui::GetIO();
			byte* font_data = null;
			int font_width = 0, font_height = 0, font_bpp = 0;
			
			io.Fonts->GetTexDataAsRGBA32(&font_data, &font_width, &font_height, &font_bpp);

			texture_create_desc<texture_2d_size> desc;
			desc.name = strings::graphics::g_imgui_mgr_name;
			desc.format = texture_format::rgba8;
			desc.size = { (u32)font_width, (u32)font_height };
			desc.usage = resource_usage::immutable;
			desc.mip_levels = 1;
			desc.generate_mips = desc.readable = false;

			texture_resource_data data{};
			data.data = font_data;
			data.stride = font_width * font_bpp;

			state.font_tex = texture_mgr_create_tex2d(desc, data);
		}

		c_str imgui_manager__get_clipboard_text(ImGuiContext* ctx)
		{
			return SDL_GetClipboardText();
		}

		void imgui_manager__set_clipboard_text(ImGuiContext* ctx, c_str text)
		{
			SDL_SetClipboardText(text);
		}

		bool imgui_manager__open_in_shell(ImGuiContext* ctx, c_str url)
		{
			return SDL_OpenURL(url) == 0;
		}

		void imgui_manager__setup_platform_handle()
		{
			auto wnd_id = g_engine_state.window_id;

			auto viewport = ImGui::GetMainViewport();
			viewport->PlatformHandle = (void*)(intptr_t)wnd_id;
		}

		void imgui_manager__on_sdl_event(events::window_event_args& args)
		{
			SDL_Event* evt = static_cast<SDL_Event*>(args.sdl_event);
			auto io = ImGui::GetIO();
			auto window_id = g_engine_state.window_id;

			switch (evt->type)
			{
				case SDL_EVENT_MOUSE_MOTION:
				{
					ImVec2 ms_pos((float)evt->motion.x, (float)evt->motion.y);
					io.AddMouseSourceEvent(evt->motion.which == SDL_TOUCH_MOUSEID ? ImGuiMouseSource_TouchScreen : ImGuiMouseSource_Mouse);
					io.AddMousePosEvent(ms_pos.x, ms_pos.y);
				}
					break;
				case SDL_EVENT_MOUSE_WHEEL:
				{
					auto& wheel = evt->wheel;
					io.AddMouseSourceEvent(evt->wheel.which == SDL_TOUCH_MOUSEID ? ImGuiMouseSource_TouchScreen : ImGuiMouseSource_Mouse);
					io.AddMouseWheelEvent(-wheel.x, wheel.y);
				}
					break;
				case SDL_EVENT_MOUSE_BUTTON_DOWN:
				case SDL_EVENT_MOUSE_BUTTON_UP:
				{
					int ms_button = -1;
					auto& btn = evt->button;
					if (btn.button == SDL_BUTTON_LEFT)
						ms_button = 0;
					if (btn.button == SDL_BUTTON_RIGHT)
						ms_button = 1;
					if (btn.button == SDL_BUTTON_MIDDLE)
						ms_button = 2;
					if (btn.button == SDL_BUTTON_X1)
						ms_button = 3;
					if (btn.button == SDL_BUTTON_X2)
						ms_button = 4;

					if (ms_button == -1)
						return;

					io.AddMouseSourceEvent(evt->button.which == SDL_TOUCH_MOUSEID ? ImGuiMouseSource_TouchScreen : ImGuiMouseSource_Mouse);
					io.AddMouseButtonEvent(ms_button, evt->type == SDL_EVENT_MOUSE_BUTTON_DOWN);
				}
					break;
				case SDL_EVENT_TEXT_INPUT:
					io.AddInputCharactersUTF8(evt->text.text);
					break;
				case SDL_EVENT_WINDOW_FOCUS_GAINED:
				case SDL_EVENT_WINDOW_FOCUS_LOST:
					io.AddFocusEvent(evt->type == SDL_EVENT_WINDOW_FOCUS_GAINED);
					break;
			}
		}
		
		void imgui_manager__update()
		{
			auto& io = ImGui::GetIO();
			const auto wnd_id = g_engine_state.window_id;
			const auto& wnd_desc = core::window_get_desc(wnd_id);
		
			io.DisplaySize = ImVec2(wnd_desc.bounds.size.x, wnd_desc.bounds.size.y);
			io.DisplayFramebufferScale = ImVec2(wnd_desc.dpi_scale.x, wnd_desc.dpi_scale.y);
			io.DeltaTime = g_engine_state.time.curr_delta;

			imgui_manager__update_mouse_data(wnd_id, wnd_desc);
			imgui_manager__update_mouse_cursor();
		}

		void imgui_manager__update_mouse_data(const core::window_t wnd_id, const core::window_desc_t& wnd_desc)
		{
			auto& io = ImGui::GetIO();
			if (!wnd_desc.focused)
				return;

			const auto& data = core::window__get_data(wnd_id);
			if (io.WantSetMousePos)
				SDL_WarpMouseInWindow(data.owner, io.MousePos.x, io.MousePos.y);

			const bool is_relative_ms_mode = SDL_GetWindowRelativeMouseMode(data.owner);
			if (is_relative_ms_mode)
				return;

			float ms_global_x, ms_global_y;
			SDL_GetGlobalMouseState(&ms_global_x, &ms_global_y);
			io.AddMousePosEvent(ms_global_x - wnd_desc.bounds.position.x, ms_global_y - wnd_desc.bounds.position.y);
		}
		
		void imgui_manager__update_mouse_cursor()
		{
			// TODO: there's no reason to made cursor handling here
			// this logic must be moved to a proper place
			auto& state = g_imgui_manager_state;
			auto& io = ImGui::GetIO();
			if (io.ConfigFlags & ImGuiConfigFlags_NoMouseCursorChange)
				return;

			auto imgui_cursor = ImGui::GetMouseCursor();
			if (io.MouseDrawCursor || imgui_cursor == ImGuiMouseCursor_None) {
				SDL_HideCursor();
				return;
			}

			SDL_Cursor* expected_cursor = state.cursors[imgui_cursor];
			if (expected_cursor != state.last_cursor)
				SDL_SetCursor(expected_cursor);

			SDL_ShowCursor();
		}
	}
}
