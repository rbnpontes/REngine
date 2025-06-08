#pragma once
#include "./defines.h"

#define VERTEX_ELEMENT_POSITION_IDX		0
#define VERTEX_ELEMENT_NORMAL_IDX		1
#define VERTEX_ELEMENT_TANGENT_IDX		2
#define VERTEX_ELEMENT_COLOR_IDX		3
#define VERTEX_ELEMENT_COLORF_IDX		4
#define VERTEX_ELEMENT_UV_IDX			5
#define VERTEX_ELEMENT_INSTANCING_IDX	6
#define VERTEX_ELEMENT_COUNT			7

#define GRAPHICS_MAX_BOUND_CBUFFERS		4
#define GRAPHICS_MAX_BOUND_TEXTURES		16

#define CORE_REQUIRED_SIZE_SCRATCH_BUFFER() \
    (CORE_ALLOC_SCRATCH_BUFFER_SIZE + \
    /* sizeof(srb_mgr_resource_desc) */ \
    (16u * GRAPHICS_MAX_BOUND_TEXTURES) + \
    /* sizeof(immutable_sampler_desc) */ \
    (40u * GRAPHICS_MAX_BOUND_TEXTURES) + \
    /* sizeof(Diligent::ImmutableSamplerDesc) */ \
    (72u * GRAPHICS_MAX_BOUND_TEXTURES) + \
    /* sizeof(Diligent::ShaderResourceVariableDesc) */ \
    (16u * GRAPHICS_MAX_BOUND_CBUFFERS) + \
    /* sizeof(Diligent::LayoutElement) */ \
    (40u * VERTEX_ELEMENT_COUNT))
