#ifndef TYPES_MODULE
#define TYPES_MODULE 1
#include "_base.module.slang"

namespace rengine {
    [Flags]
    public enum VertexLayout {
        position,
        normal,
        tangent,
        bitangent,
        color,
        uv0,
        uv1
    }

    public interface IVertexInput {
        VertexLayout getLayout();
        float3 getPosition();
        float3 getNormal();
        float3 getTangent();
        float3 getBiTangent();
        float4 getColor();
        float2 getUV0();
        float2 getUV1();
    }

    public interface IVertexConstantData {
        float4x4 getViewModel();
        float4x4 getViewProj();
    }

    public interface ITexture<T, R> {
        R sample(T coords);
    }
    typealias ITexture2D    = ITexture<float2, float4>;
    typealias ITexture3D    = ITexture<float3, float4>;
    typealias ITextureCube  = ITexture<float3, float4>;

    public struct WhiteTexture2D: ITexture2D {
        public float4 sample(float2 uv) { return float4(1); }
    }
    public struct WhiteTexture3D: ITexture3D {
        public float4 sample(float3 dir) { return float4(1); }
    }
    public struct WhiteTextureCube: ITextureCube {
        public float4 sample(float3 dir) { return float4(1); }
    }

    public struct BlackTexture2D: ITexture2D {
        public float4 sample(float2 uv) { return float4(0); }
    }
    public struct BlackTexture3D: ITexture3D {
        public float4 sample(float3 dir) { return float4(0); }
    }
    public struct BlackTextureCube: ITextureCube {
        public float4 sample(float3 dir) { return float4(0); }
    }


    public interface IMaterialInput {
        float4 getAlbedo(float2 coords);
        float3 getNormal();
        float2 getUV();
    }

    public interface IMaterial {
        float4 getAlbedo();
        float3 getNormal();
    }

    public struct StageData {
        float4 position     : SV_Position;
        float4 color        : COLOR0;
        float2 uv           : TEXCOORD0;
        // TODO: change uint to VertexLayout when slang team release
        // a new tag
        uint layout : TEXCOORD1;
    }
}
#define StageOutput StageData
#define StageInput StageData

#endif