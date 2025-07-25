#include "engine.module.slang"

using namespace rengine;
using namespace materials.unlit;

import_tex2d(g_texture);

float4 main(in StageInput input) : SV_Target {
    var material = UnlitMaterial(input, use_texture(g_texture));
    return material.getAlbedo();
}
