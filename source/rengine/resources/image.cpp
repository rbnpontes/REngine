#include "image.h"
#include "./image_private.h"
#include "../graphics/texture_manager.h"
#include "../exceptions.h"
#include "../strings.h"
#include "../io/logger.h"
#include <cstring>

namespace rengine {
        namespace resources {
                image_t* image_create(const image_create_desc& desc)
                {
                        return image__alloc(desc);
                }

                void image_destroy(image_t* image)
                {
                        image__destroy(image);
                }

                void image_get_pixelbuffer(const image_t* image, ptr* pixelbuffer_out, u32* pixelbuffer_size)
                {
                        if(!image)
                                return;

                        if(pixelbuffer_out)
                                *pixelbuffer_out = (ptr)((byte*)image + sizeof(image_t));

                        if(pixelbuffer_size)
                                *pixelbuffer_size = image->size.x * image->size.y * image->components;
                }

                void image_set_pixelbuffer(image_t* image, const ptr data)
                {
                        if(!image || !data)
                                return;

                        ptr dst; u32 size;
                        image_get_pixelbuffer(image, &dst, &size);
                        memcpy(dst, data, size);
                }

                void image_get_size(const image_t* image, math::uvec2& size)
                {
                        if(!image)
                                return;
                        size = image->size;
                }

                void image_resize(image_t* image, const math::uvec2& size)
                {
                        (void)image; (void)size;
                        throw not_implemented_exception();
                }

                void image_set_pixel(image_t* image, const image_pixel_set_desc& desc)
                {
                        if(!image)
                                return;

                        math::uvec2 pos = desc.pos;
                        image__validate_pos(image, pos);

                        ptr pixelbuffer; u32 size;
                        image_get_pixelbuffer(image, &pixelbuffer, &size);
                        byte* buf = (byte*)pixelbuffer;
                        u32 index = pos.y * image->size.x + pos.x;
                        byte* dst = buf + index * image->components;

                        switch(image->components)
                        {
                        case 1:
                                dst[0] = desc.color.r;
                                break;
                        case 2:
                                dst[0] = desc.color.r;
                                dst[1] = desc.color.g;
                                break;
                        case 3:
                                dst[0] = desc.color.r;
                                dst[1] = desc.color.g;
                                dst[2] = desc.color.b;
                                break;
                        default:
                                dst[0] = desc.color.r;
                                dst[1] = desc.color.g;
                                dst[2] = desc.color.b;
                                dst[3] = desc.color.a;
                                break;
                        }
                }

                void image_get_pixel(const image_t* image, const math::uvec2& pos, math::color& color)
                {
                        if(!image)
                                return;

                        math::uvec2 p = pos;
                        image__validate_pos(image, p);

                        ptr pixelbuffer; u32 size;
                        image_get_pixelbuffer(image, &pixelbuffer, &size);
                        byte* buf = (byte*)pixelbuffer;
                        u32 index = p.y * image->size.x + p.x;
                        byte* src = buf + index * image->components;

                        switch(image->components)
                        {
                        case 1:
                                color.r = src[0] / 255.f;
                                color.g = color.b = 0.f;
                                color.a = 1.f;
                                break;
                        case 2:
                                color.r = src[0] / 255.f;
                                color.g = src[1] / 255.f;
                                color.b = 0.f;
                                color.a = 1.f;
                                break;
                        case 3:
                                color.r = src[0] / 255.f;
                                color.g = src[1] / 255.f;
                                color.b = src[2] / 255.f;
                                color.a = 1.f;
                                break;
                        default:
                                color.r = src[0] / 255.f;
                                color.g = src[1] / 255.f;
                                color.b = src[2] / 255.f;
                                color.a = src[3] / 255.f;
                                break;
                        }
                }

                static bool image__validate_format(u8 comps, graphics::texture_format fmt)
                {
                        switch(fmt)
                        {
                        case graphics::texture_format::bc1_dxt1:
                        case graphics::texture_format::bc3_dxt5:
                        case graphics::texture_format::bc4:
                        case graphics::texture_format::bc5:
                        case graphics::texture_format::bc6h:
                        case graphics::texture_format::bc7:
                                throw resources::resource_exception(strings::exceptions::g_image_invalid_compressed_format);
                        case graphics::texture_format::r8:
                        case graphics::texture_format::d16:
                        case graphics::texture_format::d24s8:
                        case graphics::texture_format::d32s8:
                        case graphics::texture_format::d32f:
                                return comps == 1;
                        case graphics::texture_format::rg8:
                                return comps == 2;
                        default:
                                return comps >= 3 && comps <= 4;
                        }
                }

                graphics::texture_2d_t image_create_texture(const image_texture_create_desc& desc)
                {
                        if(!desc.source)
                                throw null_exception(strings::exceptions::g_image_create_texture_source_null);

                        auto* img = desc.source;
                        if(!image__validate_format(img->components, desc.format))
                                throw resources::resource_exception(strings::exceptions::g_image_invalid_format);

                        ptr pixelbuffer; u32 size;
                        image_get_pixelbuffer(img, &pixelbuffer, &size);

                        graphics::texture_create_desc<graphics::texture_2d_size> tex_desc;
                        tex_desc.name = desc.name;
                        tex_desc.size = { img->size.x, img->size.y };
                        tex_desc.format = desc.format;
                        tex_desc.usage = desc.usage;
                        tex_desc.generate_mips = desc.generate_mips;
                        tex_desc.readable = desc.readable;

                        graphics::texture_resource_data data{};
                        data.data = pixelbuffer;
                        data.stride = img->components;

                        return graphics::texture_mgr_create_tex2d(tex_desc, data);
                }
        }
}
