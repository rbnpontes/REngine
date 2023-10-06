using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver.Adapters
{
	internal class ShaderAdapter
	{
		private static readonly Diligent.ShaderType[] pShaderTypes = new Diligent.ShaderType[]
		{
			Diligent.ShaderType.Vertex,
			Diligent.ShaderType.Pixel,
			Diligent.ShaderType.Compute,
			Diligent.ShaderType.Geometry,
			Diligent.ShaderType.Hull,
			Diligent.ShaderType.Domain
		};

		public void Fill(in ShaderCreateInfo createInfo, out Diligent.ShaderCreateInfo output)
		{
			Diligent.ShaderType shaderType;
			Diligent.ShaderMacroArray macros;

			Fill(createInfo.Type, out shaderType);
			Fill(createInfo.Macros, out macros);
			
			
			output = new Diligent.ShaderCreateInfo
			{
				EntryPoint = "main",
				Desc = new Diligent.ShaderDesc 
				{ 
					UseCombinedTextureSamplers = true,
					Name = createInfo.Name,
					ShaderType = shaderType
				},
				ByteCode = createInfo.ByteCode,
				Source = createInfo.SourceCode,
				SourceLanguage = Diligent.ShaderSourceLanguage.Hlsl,
				Macros = macros
			};
		}

		public void Fill(ShaderType shaderType, out Diligent.ShaderType outShaderType)
		{
			outShaderType = pShaderTypes[(int)shaderType];
		}
		public void Fill(Diligent.ShaderType shaderType, out ShaderType outShaderType)
		{
			switch (shaderType)
			{
				case Diligent.ShaderType.Vertex:
					outShaderType = ShaderType.Vertex;
					break;
				case Diligent.ShaderType.Pixel:
					outShaderType = ShaderType.Pixel;
					break;
				case Diligent.ShaderType.Compute:
					outShaderType = ShaderType.Compute;
					break;
				case Diligent.ShaderType.Geometry:
					outShaderType = ShaderType.Geometry;
					break;
				case Diligent.ShaderType.Hull:
					outShaderType = ShaderType.Hull;
					break;
				case Diligent.ShaderType.Domain:
					outShaderType = ShaderType.Domain;
					break;
				default:
					throw new NotImplementedException($"Not implemented this shader type {shaderType}");
			}
		}
		public void Fill(ShaderTypeFlags shaderTypeFlags, out Diligent.ShaderType outShaderTypes)
		{
			Diligent.ShaderType shaderTypes = Diligent.ShaderType.Unknown;

			if ((shaderTypeFlags & ShaderTypeFlags.Vertex) != 0)
				shaderTypes = Diligent.ShaderType.Vertex;
			if ((shaderTypeFlags & ShaderTypeFlags.Pixel) != 0)
				shaderTypes |= Diligent.ShaderType.Pixel;
			if ((shaderTypeFlags & ShaderTypeFlags.Compute) != 0)
				shaderTypes |= Diligent.ShaderType.Compute;
			if ((shaderTypeFlags & ShaderTypeFlags.Geometry) != 0)
				shaderTypes |= Diligent.ShaderType.Geometry;
			if ((shaderTypeFlags & ShaderTypeFlags.Hull) != 0)
				shaderTypes |= Diligent.ShaderType.Hull;
			if ((shaderTypeFlags & ShaderTypeFlags.Domain) != 0)
				shaderTypes |= Diligent.ShaderType.Domain;

			outShaderTypes = shaderTypes;
		}
		public void Fill(IDictionary<string, string> macros, out Diligent.ShaderMacroArray output)
		{
			output = new Diligent.ShaderMacroArray
			{
				Elements = macros.Select(pair => new Diligent.ShaderMacro(pair.Key, pair.Value)).ToArray()
			};
		}
	}
}
