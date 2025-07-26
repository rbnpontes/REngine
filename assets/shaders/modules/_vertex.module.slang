#ifndef VERTEX_MODULE
#define VERTEX_MODULE 1

#include "_types.module.slang"

namespace rengine {
    public struct VertexProcessor {
        private IVertexInput _input;
        private IVertexConstantData _data;

        __init(IVertexInput input, IVertexConstantData data) {
            _input = input;
            _data = data;
        }

        public StageOutput getOutput() {
            StageOutput output;
            var layout = _input.getLayout();
            if(layout & VertexLayout.position) {
                float4x4 mvp = mul(_data.getViewModel(), _data.getViewProj());
                float4 position = float4(_input.getPosition(), 1.0f);
                output.position = mul(mvp, position);
            }

            if(layout & VertexLayout.uv0)
                output.uv = _input.getUV0();

            if(layout & VertexLayout.color)
                output.color = _input.getColor();

            output.layout = (uint)layout;
            return output;
        }
    }
}

#endif