#include <iostream>
#include <rengine/rengine.h>
#include <imgui/imgui.h>

#include <fstream>
#include <vector>
#include <filesystem>

static rengine::core::window_t g_window_id;
static rengine::graphics::texture2d_t g_test_texture;
static float g_rotation;
void game_loop() {
	if (rengine::core::window_is_destroyed(g_window_id)) {
		rengine::stop();
		return;
	}

	const auto a = rengine::math::vec3(-1, +0, 0.);
	const auto b = rengine::math::vec3(+0, -1, 0.);
	const auto c = rengine::math::vec3(+1, +0, 0.);

	const auto a_color = rengine::math::byte_color::red;
	const auto b_color = rengine::math::byte_color::green;
	const auto c_color = rengine::math::byte_color::blue;

	rengine::graphics::drawing_begin_draw();
	g_rotation += 0.1;
	//rengine::graphics::renderer_scale(12);
	rengine::graphics::drawing_scale(2);
	//rengine::graphics::renderer_rotate(g_rotation);
	rengine::graphics::drawing_translate(rengine::math::vec3(100, 100, 0));
	rengine::graphics::drawing_set_color(rengine::math::byte_color::white);
	rengine::graphics::drawing_draw_text("Hello World!!!");

	for(auto i = 0; i < 10; ++i)
	{
		rengine::graphics::drawing_set_color(rengine::math::byte_color::blue);
		rengine::graphics::drawing_scale(30.0f);
		rengine::graphics::drawing_translate(rengine::math::vec3(30.f + (i * 50.0), 30.f));
		rengine::graphics::drawing_rotate(g_rotation);

		rengine::graphics::drawing_draw_quad(rengine::math::vec3::zero, rengine::math::vec2::one);
	}

	rengine::graphics::drawing_end_draw();

	if (!rengine::graphics::imgui_manager_begin())
		return;

	if (ImGui::Begin("Test Window")) {
		ImGui::Text("Hello, world from REngine!");
		ImGui::Image(g_test_texture, ImVec2(200, 200));
		ImGui::End();
	}

	bool show_wnd;
	ImGui::ShowDemoWindow(&show_wnd);

	rengine::graphics::imgui_manager_end();
}


void load_file(rengine::c_str filepath, std::vector<rengine::byte>& data) {
	std::filesystem::path path = filepath;
	path = path.lexically_normal();
	path = path.make_preferred();

	std::ifstream file(path, std::ios::binary | std::ios::ate);
	if (!file.is_open())
		throw std::runtime_error("Failed to open file: " + std::string(filepath));

	std::streamsize size = file.tellg();
	file.seekg(0, std::ios::beg);

	data.resize(size);
	if (!file.read(reinterpret_cast<char*>(data.data()), size))
		throw std::runtime_error("Failed to read file: " + std::string(filepath));
}

rengine::graphics::texture2d_t load_test_tex() {
	rengine::c_str img_path = ENGINE_ROOT_DIR "/assets/textures/game_engine_cover.jpg";
	std::vector<rengine::byte> data;

	load_file(img_path, data);
	auto img = rengine::resources::image_decode(data.data(), data.size());

	auto texture = rengine::resources::image_create_texture({
		.name = "test_texture",
		.source = img,
		.format = rengine::graphics::texture_format::rgba8,
		.usage = rengine::graphics::resource_usage::immutable,
		.generate_mips = true,
		.readable = false,
		});

	rengine::resources::image_destroy(img);
	return texture;
}

int main(unsigned int argc, char** argv) {
	g_rotation = 0;

	rengine::init();

	const auto wnd = rengine::core::window_create("REngine", 500, 400);
	g_window_id = wnd;
	rengine::use_window(wnd);
	rengine::enable_fps_monitor();
	rengine::graphics::set_msaa_level(4);

	g_test_texture = load_test_tex();

	rengine::run(game_loop);
	rengine::destroy();
	return 0;
}