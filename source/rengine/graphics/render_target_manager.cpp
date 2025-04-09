#include "./render_target_manager.h"
#include "./render_target_manager_private.h"

namespace rengine {
	namespace graphics {
		render_target_t render_target_mgr_create(const render_target_create_info& create_desc) {
			return render_target_mgr__create(create_desc);
		}

		void render_target_mgr_destroy(const render_target_t& id) {
			render_target_mgr__destroy(id);
		}

		render_target_t render_target_mgr_resize(const render_target_t& id, const math::uvec2& size)
		{
			return render_target_mgr__resize(id, size);
		}
		
		void render_target_mgr_get_size(const render_target_t& id, math::uvec2* size) {
			render_target_mgr__get_size(id, size);
		}

		void render_target_mgr_get_desc(const render_target_t& id, render_target_desc* output_desc)
		{
			render_target_mgr__get_desc(id, output_desc);
		}

		render_target_type render_target_mgr_get_type(const render_target_t& id)
		{
			return render_target_mgr__get_type(id);
		}

		bool render_target_mgr_has_depthbuffer(const render_target_t& id)
		{
			return render_target_mgr__has_depthbuffer(id);
		}

		void render_target_mgr_get_handlers(const render_target_t& id, ptr* backbuffer, ptr* depthbuffer)
		{
			render_target_mgr__get_internal_handles(id, (Diligent::ITexture**)backbuffer, (Diligent::ITexture**)depthbuffer);
		}

		u8 render_target_mgr_get_count()
		{
			return render_target_mgr__get_count();
		}

		void render_target_mgr_clear_cache()
		{
			render_target_mgr__clear_cache();
		}
		
		void render_target_mgr_get_available_rts(u8* count, render_target_t* output_ids)
		{
			return render_target_mgr__get_available_rts(count, output_ids);
		}
		
		bool render_target_mgr_is_valid(const render_target_t& id)
		{
			return render_target_mgr__is_valid(id);
		}

		render_target_t render_target_mgr_find_from_size(const math::uvec2& size)
		{
			return render_target_mgr__find_suitable_from_size(size);
		}
	}
}