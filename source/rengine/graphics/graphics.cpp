#include "./graphics_private.h"
#include "../defines.h"
#include "../exceptions.h"
#include "../strings.h"

#include <fmt/format.h>
#include <thread>

#ifdef FEATURE_BACKEND_D3D11
	#include <EngineFactoryD3D11.h>
#endif
#ifdef FEATURE_BACKEND_D3D12
	#include <EngineFactoryD3D12.h>
#endif
#ifdef FEATURE_BACKEND_VULKAN
	#include <EngineFactoryVk.h>
#endif
#ifdef FEATURE_BACKEND_WEBGPU
	#include <EngineFactoryWebGPU.h>
#endif
#ifdef FEATURE_BACKEND_OPENGL
	#include <EngineFactoryOpenGL.h>
#endif

#define GRAPHICS_VERSION Diligent::Version { 11, 0}
namespace rengine {
	namespace graphics {
		static graphics_state g_state = {};

		graphics_state* get_state() {
			return &g_state;
		}
		void assert_backend(backend value) {
			c_str backend_str = "unknow";
			if (value < backend::max_backend)
				backend_str = g_backend_strings[(u8)value];

#ifdef FEATURE_BACKEND_D3D11
			if (value == backend::d3d11)
				return;
#endif
#ifdef FEATURE_BACKEND_D3D12
			if (value == backend::d3d12)
				return;
#endif
#ifdef FEATURE_BACKEND_VULKAN
			if (value == backend::vulkan)
				return;
#endif
#ifdef FEATURE_BACKEND_WEBGPU
			if (value == backend::webgpu)
				return;
#endif
#ifdef FEATURE_BACKEND_OPENGL
			if (value == backend::opengl)
				return;
#endif

			throw graphics_exception(
				fmt::format("Not supported this graphics backend '%s'", backend_str).c_str()
			);
		}

		void init(const graphics_init_desc& desc)
		{
			assert_backend(desc.backend);

			switch (desc.backend)
			{
			case backend::d3d11:
				init_d3d11(desc);
				break;
			case backend::d3d12:
				init_d3d12(desc);
				break;
			case backend::vulkan:
				init_vk(desc);
				break;
			case backend::webgpu:
				init_webgpu(desc);
				break;
			case backend::opengl:
				init_opengl(desc);
				break;
			}
		}

		void init_d3d11(const graphics_init_desc& desc)
		{
#ifdef FEATURE_BACKEND_D3D11
			const auto factory = Diligent::GetEngineFactoryD3D11();
			g_state.factory = factory;

			Diligent::EngineD3D11CreateInfo create_info;
			setup_engine_create_info(desc, create_info);
#endif
		}

		void init_d3d12(const graphics_init_desc& desc)
		{
#ifdef FEATURE_BACKEND_D3D12
			throw not_implemented_exception();
#endif
		}

		void init_vk(const graphics_init_desc& desc)
		{
#ifdef FEATURE_BACKEND_VULKAN
			throw not_implemented_exception();
#endif
		}

		void init_webgpu(const graphics_init_desc& desc)
		{
#ifdef FEATURE_BACKEND_WEBGPU
			throw not_implemented_exception();
#endif
		}

		void init_opengl(const graphics_init_desc& desc)
		{
#ifdef FEATURE_BACKEND_OPENGL
			throw not_implemented_exception();
#endif
		}

		void setup_engine_create_info(const graphics_init_desc& desc, Diligent::EngineCreateInfo& create_info)
		{
			create_info.GraphicsAPIVersion = GRAPHICS_VERSION;
#if _DEBUG
			create_info.EnableValidation = true;
			create_info.SetValidationLevel(Diligent::VALIDATION_LEVEL_2);
#endif
			create_info.NumImmediateContexts = 1;
			create_info.NumDeferredContexts = std::thread::hardware_concurrency();
			if (desc.backend == backend::opengl)
				create_info.NumDeferredContexts = 0;
		}

		u32 choose_best_adapter() {
			u32 num_adapters;
			g_state.factory->EnumerateAdapters(GRAPHICS_VERSION, num_adapters, null);

			if (num_adapters == 0)
				throw graphics_exception("Not found a suitable graphics card device");

			vector<Diligent::GraphicsAdapterInfo> adapters(num_adapters);
			g_state.factory->EnumerateAdapters(GRAPHICS_VERSION, num_adapters, adapters.data());

			return MAX_U32_VALUE;
		}
	}
}