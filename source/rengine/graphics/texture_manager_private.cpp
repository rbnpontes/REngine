#include "./texture_manager_private.h"
#include "./buffer_manager_private.h"
#include "./graphics_private.h"
#include "../io/logger.h"

namespace rengine {
	namespace graphics {
		texture_manager_state g_texture_mgr_state = {};

		void texture_mgr__init() {
			g_texture_mgr_state.log = io::logger_use(strings::logs::g_tex_mgr_tag);

			texture_mgr__init_dummy_white_tex2d();
		}

		void texture_mgr__deinit() {
			for (auto& entry : g_texture_mgr_state.textures_2d)
				texture_mgr_destroy_tex2d(entry.id);
			for (auto& entry : g_texture_mgr_state.textures_3d)
				texture_mgr_destroy_tex3d(entry.id);
			for (auto& entry : g_texture_mgr_state.textures_cube)
				texture_mgr_destroy_texcube(entry.id);
			for (auto& entry : g_texture_mgr_state.textures_array)
				texture_mgr_destroy_texarray(entry.id);
		}

		void texture_mgr__init_dummy_white_tex2d()
		{
			auto& state = g_texture_mgr_state;
			byte white_dummy_data[] = { 255, 255, 255, 255 }; // RGBA white pixel

			texture_create_desc<texture_2d_size> desc;
			desc.name = strings::graphics::g_texture_mgr_white_dummy_tex2d;
			desc.format = texture_format::rgba8;
			desc.size = { 1, 1 };
			desc.usage = resource_usage::immutable;
			desc.mip_levels = 1;
			desc.generate_mips = false;
			desc.readable = false;
			texture_resource_data data{};
			data.stride = 4;
			data.data = white_dummy_data;

			state.white_dummy_tex2d = texture_mgr_create_tex2d(desc, data);
		}

		void texture_mgr__fill_tex2d_desc(const texture_create_desc<texture_2d_size>& desc, Diligent::TextureDesc& out_desc)
		{
			if(desc.format == texture_format::unknown)
				throw graphics_exception(strings::exceptions::g_texture_mgr_fmt_unknown);
			else if (desc.format >= texture_format::max)
				throw graphics_exception(strings::exceptions::g_texture_mgr_fmt_invalid);

			out_desc.Name = desc.name;
			out_desc.ArraySize = 1;
			out_desc.BindFlags = Diligent::BIND_SHADER_RESOURCE;
			out_desc.Type = Diligent::RESOURCE_DIM_TEX_2D;
			out_desc.ClearValue = {};
			out_desc.MipLevels = desc.mip_levels;
			out_desc.Format = g_texture_mgr_formats[(u32)desc.format];
			out_desc.Width = desc.size.width;
			out_desc.Height = desc.size.height;
			out_desc.ImmediateContextMask = 1;
			out_desc.CPUAccessFlags = Diligent::CPU_ACCESS_NONE;
			out_desc.MiscFlags = Diligent::MISC_TEXTURE_FLAG_NONE;

			if (desc.generate_mips)
				out_desc.MiscFlags |= Diligent::MISC_TEXTURE_FLAG_GENERATE_MIPS;

			if (desc.readable)
				out_desc.CPUAccessFlags |= Diligent::CPU_ACCESS_READ;
			out_desc.CPUAccessFlags |= Diligent::CPU_ACCESS_WRITE;

			switch (desc.usage)
			{
			case resource_usage::immutable:
			{
				out_desc.Usage = Diligent::USAGE_IMMUTABLE;
				out_desc.CPUAccessFlags = Diligent::CPU_ACCESS_NONE;

				if (desc.generate_mips) {
					out_desc.Usage = Diligent::USAGE_DEFAULT;
					g_texture_mgr_state.log->warn(
						strings::exceptions::g_texture_mgr_invalid_usage_mip
					);
				}
			}
			break;
			case resource_usage::normal:
				out_desc.Usage = Diligent::USAGE_DEFAULT;
				break;
			case resource_usage::dynamic:
				out_desc.Usage = Diligent::USAGE_DYNAMIC;
				break;
			}
		}

		void texture_mgr__fill_subres(const texture_data_desc& data, Diligent::TextureSubResData* subres)
		{
			for (u32 i = 0; i < data.num_resources; ++i) {
				auto& res_output = subres[i];
				const auto& res = data.resources[i];
				res_output.pData = res.data;
				res_output.DepthStride = res.depth_stride;
				res_output.SrcOffset = res.src_offset;
				res_output.Stride = res.stride;
			}
		}

		Diligent::ITexture* texture_mgr__create(Diligent::TextureDesc& desc, Diligent::TextureData& data, bool gen_mipmap)
		{
			auto device = g_graphics_state.device;
			auto ctx = g_graphics_state.contexts[0];
			Diligent::ITexture* texture = nullptr;
			device->CreateTexture(desc, data.NumSubresources == 0 ? null : &data, &texture);

			if (!texture)
				throw graphics_exception(
					fmt::format(strings::exceptions::g_texture_mgr_failed_to_create_tex, desc.Name).c_str()
				);

			if (gen_mipmap)
				ctx->GenerateMips(texture->GetDefaultView(Diligent::TEXTURE_VIEW_SHADER_RESOURCE));
			return texture;
		}

		void texture_mgr__get_internal_handle(texture_type type, u16 id, Diligent::ITexture** output)
		{
			if (!output)
				return;

			*output = null;

			auto& state = g_texture_mgr_state;

			switch (type)
			{
			case texture_type::tex2d:
				if (id != no_texture_2d)
					*output = state.textures_2d[id].value.handler;
				break;
			case texture_type::tex3d:
				if (id != no_texture_3d)
					*output = state.textures_3d[id].value.handler;
				break;
			case texture_type::texcube:
				if (id != no_texture_cube)
					*output = state.textures_cube[id].value.handler;
				break;
			case texture_type::texarray:
				if (id != no_texture_array)
					*output = state.textures_array[id].value.handler;
				break;
			default:
				break;
			}
		}
	}
}