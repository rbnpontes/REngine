using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public enum PrimitiveType
	{
		TriangleList = 0,
		LineLine,
		PointList,
		TriangleStrip,
		LineStrip
	}
	public enum BlendMode
	{
		Replace =0,
		Add,
		Multiply,
		Alpha,
		AddAlpha,
		PreMulAlpha,
		InvDestAlpha,
		Subtract,
		SubtractAlpha,
		DeferredDecal
	}
	public enum CompareMode
	{
		Always,
		Equal,
		NotEqual,
		Less,
		LessEqual,
		Greater,
		GreaterEqual
	}
	public enum CullMode
	{
		Both,
		Back,
		Front,
	}
	public enum FillMode
	{
		Solid,
		WireFrame,
	}
	public enum StencilOp
	{
		Keep,
		Zero,
		Ref,
		Incr,
		Decr
	}
	public enum PipelineType
	{
		Graphics=0,
		Compute,
		Mesh,
		RayTracing,
		Tile
	}
	public enum ElementType
	{
		Int,
		Float,
		Vector2,
		Vector3,
		Vector4,
		UByte4,
		UByte4Norm
	}
	public enum ElementSemantic
	{
		Position,
		Normal,
		BiNormal,
		Tangent,
		Texcoord,
		Color,
		BlendWeights,
		BlendIndices,
		ObjectIndex
	}
	public enum TextureFormat
	{
		Unknown,
		RGBA32Typeless,
		RGBA32Float,
		RGBA32UInt,
		RGBA32SInt,
		RGB32Typeless,
		RGB32Float,
		RGB32UInt,
		RGB32SInt,
		RGBA16Typeless,
		RGBA16Float,
		RGBA16UNorm,
		RGBA16UInt,
		RGBA16SNorm,
		RGBA16SInt,
		RG32Typeless,
		RG32Float,
		RG32UInt,
		RG32SInt,
		R32G8X24Typeless,
		D32FloatS8X24UInt,
		R32FloatX8X24Typeless,
		X32TypelessG8X24UInt,
		RGB10A2Typeless,
		RGB10A2UNorm,
		RGB10A2UInt,
		R11G11B10Float,
		RGBA8Typeless,
		RGBA8UNorm,
		RGBA8UNormSRGB,
		RGBA8UInt,
		RGBA8SNorm,
		RGBA8SInt,
		RG16Typeless,
		RG16Float,
		RG16UNorm,
		RG16UInt,
		RG16SNorm,
		RG16SInt,
		R32Typeless,
		D32Float,
		R32Float,
		R32UInt,
		R32SInt,
		R24G8Typeless,
		D24UNormS8UInt,
		R24UNormX8Typeless,
		X24TypelessG8UInt,
		RG8Typeless,
		RG8UNorm,
		RG8UInt,
		RG8SNorm,
		RG8SInt,
		R16Typeless,
		R16Float,
		D16UNorm,
		R16UNorm,
		R16UInt,
		R16SNorm,
		R16SInt,
		R8Typeless,
		R8UNorm,
		R8UInt,
		R8SNorm,
		R8SInt,
		A8UNorm,
		R1UNorm,
		RGB9E5Sharedexr,
		RG8B8G8UNorm,
		G8R8G8B8UNorm,
		BC1Typeless,
		BC1UNorm,
		BC1UNormSRGB,
		BC2Typeless,
		BC2UNorm,
		BC2UNormSRGB,
		BC3Typeless,
		BC3UNorm,
		BC3UNormSRGB,
		BC4Typeless,
		BC4UNorm,
		BC4SNorm,
		BC5Typeless,
		BC5UNorm,
		BC5SNorm,
		B5G6R5UNorm,
		B5G5R5A1UNorm,
		BGRA8UNorm,
		BGRX8UNorm,
		R10G10B10XrBiasA2UNorm,
		BGRA8Typeless,
		BGRA8UNormSRGB,
		BGRX8Typeless,
		BGRX8UNormSRGB,
		BC6HTypeless,
		BC6HUf16,
		BC6HSf16,
		BC7Typeless,
		BC7UNorm,
		BC7UNormSRGB,
	}
	public enum TextureFilterMode
	{
		Nearest,
		Bilinear,
		Trilinear,
		Anisotropic,
		NearestAnisotropic,
		Default
	}
	public enum TextureAddressMode
	{
		Wrap,
		Mirror,
		Clamp
	}
	public enum ShaderType
	{
		Vertex,
		Pixel,
		Compute,
		Geometry,
		Hull,
		Domain
	}

	public enum ShaderTypeFlags
	{
		None = 0,
		Vertex = 1 << 0,
		Pixel = 1 << 1,
		Compute = 1 << 2,
		Geometry = 1 << 3,
		Hull = 1 << 4,
		Domain = 1 << 5,
		VertexAndPixel = Vertex | Pixel,
		All = Vertex | Pixel | Compute | Geometry | Hull | Domain
	}

	[Flags]
	public enum ClearDepthStencil
	{
		None = 0x0,
		Depth = 0x1,
		Stencil = 0x2
	}

	[Flags]
	public enum BindFlags
	{
		None = 0,
		VertexBuffer = 1 << 0,
		IndexBuffer = 1 << 1,
		UniformBuffer = 1 << 2,
		ShaderResource = 1 << 3,
		StreamOutput = 1 << 4,
		RenderTarget = 1 << 5,
		DepthStencil = 1 << 6,
		UnorderedAccess = 1 << 7,
		IndirectDrawArgs = 1 << 8,
		InputAttachment = 1 << 9,
		RayTracing = 1 << 10,
		ShadingRate = 1 << 11
	}

	[Flags]
	public enum CpuAccessFlags
	{
		None =0,
		Read = 1 << 0,
		Write = 1 << 1
	}

	public enum BufferMode
	{
		Undefined,
		Formatted,
		Structured,
		Raw
	}

	public enum Usage
	{
		Immutable =0,
		Default,
		Dynamic,
		Staging,
		Unified,
		Sparse
	}

	public enum MapType
	{
		Read = 1,
		Write,
		ReadWrite
	}
	[Flags]
	public enum MapFlags
	{
		None = 0x0,
		DontWait = 0x1,
		Discard = 0x2,
		NoOverwrite = 0x4
	}

	public enum ValueType : byte
	{
		Undefined,
		Int8,
		Int16,
		Int32,
		UInt8,
		UInt16,
		UInt32,
		Float16,
		Float32,
		Float64,
		NumTypes
	}
}
