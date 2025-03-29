#include "./graphics_private.h"
#include "./graphics_utils_private.h"

#include "../defines.h"
#include "../exceptions.h"
#include "../strings.h"
#include "../io/logger.h"

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
				backend_str = strings::g_backend_strings[(u8)value];

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
			g_state.backend = desc.backend;

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

			g_state.num_contexts = create_info.NumImmediateContexts + create_info.NumDeferredContexts;
			factory->CreateDeviceAndContextsD3D11(create_info, &g_state.device, &g_state.contexts);
			
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
			create_info.AdapterId = choose_best_adapter(desc.adapter_id);
			create_info.NumImmediateContexts = 1;
			create_info.NumDeferredContexts = std::thread::hardware_concurrency();
			if (desc.backend == backend::opengl)
				create_info.NumDeferredContexts = 0;
		}

		u32 choose_best_adapter(u8 preferred_id) {
			u32 num_adapters;
			g_state.factory->EnumerateAdapters(GRAPHICS_VERSION, num_adapters, null);

			if (num_adapters == 0)
				throw graphics_exception(strings::exceptions::g_graphics_not_suitable_device);

			vector<Diligent::GraphicsAdapterInfo> adapters(num_adapters);
			g_state.factory->EnumerateAdapters(GRAPHICS_VERSION, num_adapters, adapters.data());

			if (preferred_id != MAX_U8_VALUE && preferred_id < adapters.size()) {
				const auto& adapter = adapters[preferred_id];
				if (adapter.Type == adapter.Type == Diligent::ADAPTER_TYPE_UNKNOWN)
					throw graphics_exception(strings::exceptions::g_graphics_unknow_adapter);
				return preferred_id;
			} else {
				io::logger_warn(strings::logs::g_graphics_tag, fmt::format(strings::logs::g_graphics_invalid_adapter_id, preferred_id).c_str());
			}
			
			u32 result = utils::choose_adapter_with_best_memory(adapters, Diligent::ADAPTER_TYPE_DISCRETE);
			if (result != MAX_U32_VALUE)
				return result;

			result = utils::choose_adapter_with_best_memory(adapters, Diligent::ADAPTER_TYPE_INTEGRATED);
			if (result != MAX_U32_VALUE)
				return result;

			result = 0;
			io::logger_warn(strings::logs::g_graphics_tag, strings::logs::g_graphics_no_suitable_device_found);
			return result;
		}
	}
}