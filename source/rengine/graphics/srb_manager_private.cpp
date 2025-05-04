#include "./srb_manager_private.h"
#include "./render_target_manager.h"
#include "./buffer_manager_private.h"

#include "../core/hash.h"
#include "../exceptions.h"

namespace rengine {
	namespace graphics {
		srb_mgr_state g_srb_mgr_state = {};

		void srb_mgr__init()
		{
			g_srb_mgr_state.log = io::logger_use(strings::logs::g_srb_cmd_tag);
		}

		void srb_mgr__deinit()
		{
			srb_mgr_clear_cache();
		}

		bool srb_mgr__assert_id(srb_t id)
		{
			bool result;
			if(!(result = id < g_srb_mgr_state.entries.size()))
				g_srb_mgr_state.log->warn(
					fmt::format(strings::logs::g_srb_mgr_invalid_id, id).c_str()
				);

			return result;
		}

		Diligent::IShaderResourceBinding* srb_mgr__create(Diligent::IPipelineState* pipeline, const srb_mgr_create_desc& desc)
		{
			Diligent::IShaderResourceBinding* srb = null;
			pipeline->CreateShaderResourceBinding(&srb, true);

			if (!srb)
				return null;

			srb->AddRef();

			srb_mgr__set_resources(srb, desc.resources, desc.num_resources);
			return srb;
		}

		core::hash_t srb_mgr__build_hash(const pipeline_state_t pipeline_id, const srb_mgr_resource_desc* resources, const u8 num_resources)
		{
			auto hash = (core::hash_t)pipeline_id;
			for (u8 i = 0; i < num_resources; ++i) {
				const auto& resource = resources[i];
				hash = core::hash_combine(hash, core::hash(resource.name));
				hash = core::hash_combine(hash, resource.id);
				hash = core::hash_combine(hash, (u32)resource.type);
			}

			return hash;
		}

		void srb_mgr__get_handle(const srb_t& id, Diligent::IShaderResourceBinding** output)
		{
			const auto& state = g_srb_mgr_state;
			if (!srb_mgr__assert_id(id))
				return;

			*output = state.entries[id].handle;
		}

		Diligent::IDeviceObject* srb_mgr__get_device_obj(const srb_mgr_resource_desc& resource)
		{
			Diligent::IDeviceObject* result = null;
			switch (resource.type)
			{
			case srb_mgr_resource_type::tex2d:
				throw not_implemented_exception();
				break;
			case srb_mgr_resource_type::tex3d:
				throw not_implemented_exception();
				break;
			case srb_mgr_resource_type::texcube:
				throw not_implemented_exception();
				break;
			case srb_mgr_resource_type::texarray:
				throw not_implemented_exception();
				break;
			case srb_mgr_resource_type::rt:
				render_target_mgr_get_handlers(resource.id, reinterpret_cast<ptr*>(&result), null);
				break;
			}

			return result;
		}
		
		void srb_mgr__set_resources(Diligent::IShaderResourceBinding* srb, const srb_mgr_resource_desc* resources, u8 num_resources)
		{
			Diligent::SHADER_TYPE shader_types[] = {
				Diligent::SHADER_TYPE_VERTEX,
				Diligent::SHADER_TYPE_PIXEL
			};
			for (u8 i = 0; i < num_resources; ++i) {
				const auto& resource = resources[i];
				auto obj = srb_mgr__get_device_obj(resource);

				for (u8 j = 0; j < _countof(shader_types); ++j) {
					const auto& shader_type = shader_types[j];
					auto var = srb->GetVariableByName(shader_type, resource.name);

					if (var)
						var->Set(obj, Diligent::SET_SHADER_RESOURCE_FLAG_ALLOW_OVERWRITE);
				}
			}
		}
	}
}