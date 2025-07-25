#include "engine.module.slang"

using namespace rengine;
using namespace materials.unlit;

[shader("pixel")]
float4 main(in StageInput input) : SV_Target {
    var material = UnlitMaterial(input, WhiteTexture2D());
    return material.getAlbedo();
}
