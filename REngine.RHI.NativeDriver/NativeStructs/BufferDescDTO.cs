using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct BufferDescDTO
	{
		public IntPtr name;
		public ulong size;
		public uint bindFlags;
		public byte usage;
		public byte accessFlags;
		public byte mode;
		public uint elementByteStride;

		public static void Fill(in BufferDesc desc, out BufferDescDTO output)
		{
			output = new BufferDescDTO()
			{
				name = string.IsNullOrEmpty(desc.Name) ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(desc.Name),
				size = desc.Size,
				bindFlags = (uint)desc.BindFlags,
				usage = (byte)desc.Usage,
				accessFlags = (byte)desc.AccessFlags,
				mode = (byte)desc.Mode,
				elementByteStride = desc.ElementByteStride,
			};
		}
		public static void Fill(in BufferDescDTO desc, ref BufferDesc output)
		{
			output.Name = string.Intern(Marshal.PtrToStringAnsi(desc.name) ?? string.Empty);
			output.Size = desc.size;
			output.BindFlags = (BindFlags)desc.bindFlags;
			output.Usage = (Usage)desc.usage;
			output.AccessFlags = (CpuAccessFlags)desc.accessFlags;
			output.Mode = (BufferMode)desc.mode;
			output.ElementByteStride = desc.elementByteStride;
		}
	}
}
