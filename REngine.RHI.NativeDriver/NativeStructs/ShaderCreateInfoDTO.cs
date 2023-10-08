using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal unsafe struct ShaderCreateInfoDTO
	{
		public IntPtr name;
		public byte type;
		public IntPtr sourceCode;
		public IntPtr byteCode;
		public uint byteCodeLength;
		public IntPtr* macroKeys;
		public IntPtr* macroValues;
		public uint numMacros;

		public static void Fill(in ShaderCreateInfo ci, out StringArray macroKeys, out StringArray macroValues, out ShaderCreateInfoDTO output)
		{
			macroKeys = new StringArray(ci.Macros.Keys.ToArray());
			macroValues = new StringArray(ci.Macros.Values.ToArray());
			output = new ShaderCreateInfoDTO
			{
				name = string.IsNullOrEmpty(ci.Name) ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(ci.Name),
				type = (byte)ci.Type,
				sourceCode = string.IsNullOrEmpty(ci.SourceCode) ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(ci.SourceCode),
				byteCodeLength = (uint)ci.ByteCode.Length,
				macroKeys = macroKeys.Handle,
				macroValues = macroValues.Handle,
				numMacros = (uint)ci.Macros.Keys.Count
			};
		}
	}
}
