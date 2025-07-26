#include "engine.module.slang"

using namespace rengine;
using namespace materials.unlit;

struct FrameData: IVertexConstantData {
    float4x4 screen_projection;
    float4x4 getViewModel() { return s_matrix4x4_identity; }
    float4x4 getViewProj() { return screen_projection; }
};
ParameterBlock<FrameData> g_frame;

[shader("vertex")]
StageOutput main(in UnlitTextureVertexInput input) {
    var processor = VertexProcessor(input, g_frame);
    return processor.getOutput();
}
