using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct CopyTextureInfoDTO
	{
		public IntPtr srcTexture;
		public uint srcMipLevel;
		public uint srcSlice;
		public IntPtr srcBox;
		public IntPtr dstTexture;
		public uint dstMipLevel;
		public uint dstSlice;
		public uint dstX;
		public uint dstY;
		public uint dstZ;

		public static void Fill(in CopyTextureInfo copy, out CopyTextureInfoDTO output)
		{
			output = new CopyTextureInfoDTO
			{
				srcTexture = copy.SrcTexture?.Handle ?? IntPtr.Zero,
				srcMipLevel = copy.SrcMipLevel,
				srcSlice = copy.SrcSlice,
				srcBox = IntPtr.Zero,
				dstTexture = copy.DstTexture?.Handle ?? IntPtr.Zero,
				dstMipLevel = copy.DstMipLevel,
				dstSlice = copy.DstSlice,
				dstX = copy.DstX,
				dstY = copy.DstY,
				dstZ = copy.DstZ
			};
		}
	}
}
