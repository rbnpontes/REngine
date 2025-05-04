#pragma once
#include "../base_private.h"
#include "./srb_manager.h"

#include "../io/logger.h"

#include <ShaderResourceBinding.h>
#include <PipelineState.h>

namespace rengine {
	namespace graphics {
		struct srb_mgr_entry {
			core::hash_t id;
			Diligent::IShaderResourceBinding* handle;
			pipeline_state_t pipeline;
		};
		
		typedef hash_map<srb_t, u32> srb_mgr_tbl;

		struct srb_mgr_state {
			vector<srb_mgr_entry> entries;
			srb_mgr_tbl srb_tbl;
			io::ILog* log;
		};
		extern srb_mgr_state g_srb_mgr_state;

		void srb_mgr__init();
		void srb_mgr__deinit();

		bool srb_mgr__assert_id(srb_t id);

		Diligent::IShaderResourceBinding* srb_mgr__create(Diligent::IPipelineState* pipeline, const srb_mgr_create_desc& desc);
		core::hash_t srb_mgr__build_hash(const pipeline_state_t pipeline_id, const srb_mgr_resource_desc* resources, const u8 num_resources);
		void srb_mgr__get_handle(const srb_t& id, Diligent::IShaderResourceBinding** output);

		Diligent::IDeviceObject* srb_mgr__get_device_obj(const srb_mgr_resource_desc& resource);

		void srb_mgr__set_resources(Diligent::IShaderResourceBinding* srb, const srb_mgr_resource_desc* resources, u8 num_resources);
	}
}